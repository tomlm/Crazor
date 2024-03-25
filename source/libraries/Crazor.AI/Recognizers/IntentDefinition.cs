﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.AI.Recognizers
{
    public class IntentDefinition
    {
        [JsonConstructor]
        public IntentDefinition()
        { }

        public IntentDefinition(string intent, string description, params string[] examples)
        {
            Intent = intent;
            Description= description;
            if (examples != null)
            {
                Examples = examples.ToList();
            }
        }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("examples")]
        public List<string> Examples { get; set; } = new List<string>();
    }
}
