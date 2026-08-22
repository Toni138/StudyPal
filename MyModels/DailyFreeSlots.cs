using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels
{
    public class DailyFreeSlots
    {
        public DayOfWeek Day { get; set; }

        public List<TimeSlot> FreeSlots { get; set; } = new List<TimeSlot>();
    }
}
