using Crazor;
using Crazor.Attributes;

namespace SharedCards.Cards.Dice
{
    [TaskInfo(Title = "Dice", Width = "medium", Height = "medium")]
    public class DiceApp : CardApp
    {
        public DiceApp(CardAppContext context) : base(context)
        {
        }

    }
}
