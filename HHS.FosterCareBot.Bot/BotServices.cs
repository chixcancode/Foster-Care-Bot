// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;

namespace HHS.FosterCareBot.Bot
{
    public class BotServices : IBotServices
    {
        public BotServices(IConfiguration configuration)
        {
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            Dispatch = new LuisRecognizer(new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
                true);

            FosterCareBotQnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAAuthKey"],
                Host = configuration["QnAEndpointHostName"]
            });

            SendFromEmail = configuration["sendFromEmail"];
            SendToEmail = configuration["sendToEmail"];
            SendToEmailPassword = configuration["sendFromEmailPassword"];
        }

        public LuisRecognizer Dispatch { get; private set; }

        public QnAMaker FosterCareBotQnA { get; private set; }

        public string SendToEmail { get; private set; }

        public string SendFromEmail { get; private set; }

        public string SendToEmailPassword { get; private set; }
    }
}