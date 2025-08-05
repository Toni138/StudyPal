using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class AppSessionValidator
    {
        public static readonly string AppRestartToken = Guid.NewGuid().ToString();
    }
}
