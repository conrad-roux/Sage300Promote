using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sage.CRM.WebObject;

namespace DebugPromote
{
    public static class AppFactory
    {
        public static void GetPromotePage(ref Web AretVal)
        {
            AretVal = new PromoteCompany();
        }
    }
}
