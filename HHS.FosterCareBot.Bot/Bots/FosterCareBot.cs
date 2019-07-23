// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HHS.FosterCareBot.Bot.Dialogs;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;


namespace HHS.FosterCareBot.Bot
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class FosterCareBot : ActivityHandler
    {
        private const string WelcomeText = "I can help you along your journey as a Foster Parent by answering a number of questions.  How can I help you today?";
        private const string DefaultNoAnswer = "Sorry I wasn't able to find a great answer.";

        private readonly IBotServices _services;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<CourtDatesState> _courtDateStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly ILogger _logger;
  
        private DialogSet _dialogs;
       
        /// <summary>
        /// Initializes a new instance of the <see cref="FosterCareBot"/> class.
        /// </summary>
        /// <param name="services">Services configured from the ".bot" file.</param>
        public FosterCareBot(IBotServices services, UserState userState, ConversationState conversationState, ILogger<FosterCareBot> logger)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _courtDateStateAccessor = _userState.CreateProperty<CourtDatesState>(nameof(CourtDatesState));
            _logger = logger;
            _dialogs = new DialogSet(_dialogStateAccessor);
            _dialogs.Add(new ChildCareProvidersDialog());
            _dialogs.Add(new UpcomingCourtDatesDialog(_courtDateStateAccessor, logger, _services));
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            DialogContext dialogContext = await this._dialogs.CreateContextAsync(turnContext, cancellationToken);
            if (dialogContext.ActiveDialog != null)
            {
                var dialogResult = await dialogContext.ContinueDialogAsync();
            }
            else { 
                // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
                var recognizerResult = await _services.Dispatch.RecognizeAsync(turnContext, cancellationToken);

            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();

            // Next, we call the dispatcher with the top intent.
            await DispatchToTopIntentAsync(turnContext, topIntent.intent, recognizerResult, cancellationToken);
        }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }
      
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, turnContext.Activity.From.Name, cancellationToken);
        }

        /// <summary>
        /// Depending on the intent from Dispatch, routes to the right LUIS model or QnA service.
        /// </summary>
        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, RecognizerResult recognizerResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string childWelfareLUISDispatchKey = "l_FosterCare";
            const string noneDispatchKey = "None";
            const string qnaDispatchKey = "q_FosterCare-qna";

            switch (intent)
            {
                case childWelfareLUISDispatchKey:
                    await DispatchToLuisModelAsync(turnContext, recognizerResult.Properties["luisResult"] as LuisResult, cancellationToken);
                  break;
                case noneDispatchKey:
                // You can provide logic here to handle the known None intent (none of the above).
                // In this example we fall through to the QnA intent.
                case qnaDispatchKey:
                    await DispatchToQnAMakerAsync(turnContext, cancellationToken);
                    break;

                default:
                    // The intent didn't match any case, so just display the recognition results.
                    await turnContext.SendActivityAsync(DefaultNoAnswer);
                    break;
            }
        }

        /// <summary>
        /// Dispatches the turn to the request QnAMaker app.
        /// </summary>
        private async Task DispatchToQnAMakerAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var results = await _services.FosterCareBotQnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(DefaultNoAnswer), cancellationToken);
            }
        }

        /// <summary>
        /// Dispatches the turn to the requested LUIS model.
        /// </summary>
        private async Task DispatchToLuisModelAsync(ITurnContext<IMessageActivity> turnContext, LuisResult luisResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            // Retrieve LUIS result for Process Automation.
            var result = luisResult.ConnectedServiceResult;
            var intent = result.TopScoringIntent.Intent;

            switch (intent)
            {
                case "GetChildCareProviders":
                    await dialogContext.ReplaceDialogAsync(ChildCareProvidersDialog.Name, cancellationToken);
                    break;
                    break;
                case "GetUpcomingCourtDates":
                   await dialogContext.ReplaceDialogAsync(UpcomingCourtDatesDialog.Name, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    break;
                default:
                    await turnContext.SendActivityAsync(DefaultNoAnswer);
                    break;
            }
            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, string name, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity.CreateReply();
            activity.Text = $"Hello {name}. {WelcomeText}";
            activity.Speak = $"Hey {name}. {WelcomeText}";
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
    }
}
