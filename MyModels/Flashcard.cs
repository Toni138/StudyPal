using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MyModels
{
    public class Flashcard
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
     
        [BindNever]
        [ValidateNever]
        public User User { get; set; }
        [Required]
        public  string Question { get; set; }
        [Required]
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
  
        public DateTime NextReviewTime { get; set; }
        public string? Tag { get; set; }
    }
}
