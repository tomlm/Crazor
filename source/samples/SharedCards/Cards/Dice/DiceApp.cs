using Crazor;
using Crazor.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SharedCards.Cards.Dice
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
        [FromCardRoute]
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
