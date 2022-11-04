using System.ComponentModel.DataAnnotations;

namespace OpBot.Cards.WorkOrder
{
    public class Asset
    {
        public Asset()
        {

        }
        public Asset(string name)
        {
            Name = name;
        }

        [Required]
        public string? Name { get; set; }
    }
}
