using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor
{
    public class CardViewState
    {
        public CardViewState(string view, object? state=null)
        {
            Name = view;
            if (state is CardApp)
            {
                Model = null;
            }
            else
            {
                Model = state;
            }
        }

        public string Name { get; set; } = String.Empty;

        public object? Model { get; set; } = null;
    }
}
