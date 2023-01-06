// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CrazorBlazorDemo.Cards.Dice
{
    [TaskInfo(Title = "Dice", Width = "medium", Height = "medium")]
    public class DiceApp : CardApp
    {
        public DiceApp(CardAppContext context) : base(context)
        {
        }

        /// <summary>
        /// the dice name is in the route, /cards/dice/{dicename}
        /// </summary>
        [FromRoute]
        [Required]
        public string? DiceName { get; set; }

        [Required]
        [Description("Number of dice")]
        [Range(1, 50, ErrorMessage = "Dice must be between 1 and 50.")]
        [PathMemory(nameof(DiceName))]
        public int? NumberDice { get; set; }

        [PathMemory(nameof(DiceName))]
        public List<int>? Dice { get; set; }

        public void RollDice()
        {
            if (NumberDice.HasValue)
            {
                Random rnd = new Random();
                Dice = Enumerable.Range(1, this.NumberDice.Value).Select(a => rnd.Next(1, 6)).ToList();
            }
        }
    }
}
