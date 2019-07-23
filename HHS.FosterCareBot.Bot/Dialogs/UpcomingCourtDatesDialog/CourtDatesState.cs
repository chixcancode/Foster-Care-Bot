using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HHS.FosterCareBot.Bot.Dialogs
{
    public class CourtDatesState
    {
        public DateTime CourtDate { get; set; }
        public bool RequestedReminder { get; set; }
    }
}
