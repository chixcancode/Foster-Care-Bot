using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace HHS.FosterCareBot
{
    public interface IBotServices
    {
        LuisRecognizer Dispatch { get; }
        QnAMaker FosterCareBotQnA { get; }
        string SendToEmail { get; }
        string SendFromEmail { get; }
        string SendToEmailPassword { get; }
    }
}