using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Blazor.Components.Adaptive
{
    public class ActionOK : ActionExecute
    {
        public ActionOK()
        {
            Title = "OK";
            Verb = Constants.OK_VERB;
        }
    }
}
