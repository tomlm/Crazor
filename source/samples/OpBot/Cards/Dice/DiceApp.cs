using Azure.Storage.Blobs.Models;
using Crazor;
using Crazor.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpBot.Cards.Dice
{
    public class DiceApp : CardApp
    {
        public DiceApp(IServiceProvider services) : base(services)
        {
        }

        [SharedMemory]
        [Required]
        [Description("Number of dice")]
        [Range(1, 50, ErrorMessage = "Dice must be between 1 and 50.")]
        public int? NumberDice { get; set; }

        [SharedMemory]
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
