using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SharedCards.Cards.Dice
{
    public class DiceModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Description("Number of dice")]
        [Range(1, 50, ErrorMessage = "Dice must be between 1 and 50.")]
        public int? Number { get; set; }

        public List<int>? Values { get; set; }

        public void RollDice()
        {
            if (Number.HasValue)
            {
                Random rnd = new Random();
                Values = Enumerable.Range(1, this.Number.Value).Select(a => rnd.Next(1, 6)).ToList();
            }
        }
    }
}
