using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyModels
{
    public class LoginViewModel
    {
        public required string UsernameorEmail { get; set; }
        public required string Password { get; set; }
    }
}
