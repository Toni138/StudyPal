using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels;

public class TimetableInputModel
{
    public List<Subject> Subjects { get; set; } = new List<Subject>();

    public List<DailyFreeSlots> DailyFreeSlots { get; set; } = new List<DailyFreeSlots>();
}