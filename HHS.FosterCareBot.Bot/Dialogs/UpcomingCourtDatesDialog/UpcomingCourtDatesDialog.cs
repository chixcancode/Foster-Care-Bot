using System;
using System.Threading;
using System.Threading.Tasks;
using HHS.FosterCareBot.Business;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HHS.FosterCareBot.Bot.Dialogs
{

    public class UpcomingCourtDatesDialog : ComponentDialog
    {
        public const string Name = "UpcomingCourtDates";
        public IStatePropertyAccessor<CourtDatesState> UserProfileAccessor { get; }

        // Prompts names
        private const string SendCourtDatePrompt = "sendCourtDatePrompt";
        private const string CourtDateDialog = "CourtDateDialog";
        private const string SendConfirmationPrompt = "sendConfirmationPrompt";
        private IBotServices _services;
       
        public UpcomingCourtDatesDialog(IStatePropertyAccessor<CourtDatesState> userProfileStateAccessor, ILogger logger, IBotServices services)
           : base(Name)
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));
            _services = services;
            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                   InitializeStateStepAsync,
                   PromptforSendCourtDate,
                   SendConfirmation
            };
            AddDialog(new WaterfallDialog(CourtDateDialog, waterfallSteps));
            AddDialog(new ConfirmPrompt(SendCourtDatePrompt));
            AddDialog(new TextPrompt(SendConfirmationPrompt));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfileState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (userProfileState == null)
            {
                var userProfileStateOpt = stepContext.Options as CourtDatesState;
                if (userProfileStateOpt != null)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, userProfileStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new CourtDatesState());
                }
            }

            return await stepContext.NextAsync();
        }
        private async Task<DialogTurnResult> PromptforSendCourtDate(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var courtDateState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var courtDateString = DateTime.Now.AddDays(7).ToLongDateString();
            string response = "I've found 1 upcoming court date on " + courtDateString + ".  Would you like me to send you an appointment?";

            courtDateState.CourtDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
            await UserProfileAccessor.SetAsync(stepContext.Context, courtDateState);

            var activity = stepContext.Context.Activity.CreateReply();
            activity.Speak = response;
            activity.Text = response;

            var retryActivity = stepContext.Context.Activity.CreateReply();
            retryActivity.Speak = "That's not a valid choice. You can say either Yes or No";
            retryActivity.Text = "That's not a vaild choice, please select either Yes or No";

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
            return await stepContext.PromptAsync(SendCourtDatePrompt, new PromptOptions { Prompt = activity, RetryPrompt = retryActivity}, cancellationToken);

        }

        private async Task<DialogTurnResult> SendConfirmation(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var courtDateState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var activity = stepContext.Context.Activity.CreateReply();
             
            bool sendConfirmation = bool.Parse(stepContext.Result.ToString());
            if(sendConfirmation)
            {
                Email.SendAppointment(courtDateState.CourtDate, courtDateState.CourtDate.AddHours(2), _services.SendToEmail, _services.SendFromEmail, _services.SendToEmailPassword);
                activity.Text = "Ok, I have sent calendar appointment.";
                activity.Speak = "Ok, I have sent calendar appointment.";
            }
            else
            {
                activity.Text = "Ok, I won't send appointment.";
                activity.Speak = "Ok, I won't send appointment.";
            }
            await stepContext.Context.SendActivityAsync(activity).ConfigureAwait(false);
            return await stepContext.EndDialogAsync();
        }
    }
}
