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
        public CardViewState(string view, object? model=null)
        {
            Name = view;
            if (model is CardApp)
            {
                throw new ArgumentException("CardApp can't be the model");
            }
            else
            {
                Model = model;
            }
        }

        // name of the card view
        public string Name { get; set; } = String.Empty;

        // card view model
        public object? Model { get; set; } = null;

        // any cardview [sessionMemory] properties are stored here.
        public Dictionary<string, object> SessionMemory { get; set; } = new Dictionary<string, object>();
    }
}
