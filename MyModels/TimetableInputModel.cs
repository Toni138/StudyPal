using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels;

public class TimetableInputModel
{
    public List<Subject> Subjects { get; set; } = new List<Subject>();
    public List<DailyStudyHours> DailyStudyHours { get; set; } = new List<DailyStudyHours>();
}