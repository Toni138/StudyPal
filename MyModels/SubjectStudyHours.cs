using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels
{
    public class SubjectStudyHours
    {
        public int Id { get; set; } 

        public Guid UserStatsId { get; set; } 
        public UserStats UserStats { get; set; } 

        public string SubjectName { get; set; }
        public int Hours { get; set; }
    }
}
