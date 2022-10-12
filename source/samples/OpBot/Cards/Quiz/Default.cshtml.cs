using Crazor.Attributes;
using System.Reflection;

namespace OpBot.Cards.Quiz
{
    public class DefaultModel
    {
        public DefaultModel()
        {
            NextQuestion();
        }

        public int CorrectGuesses { get; set; } = 0;

        public int TotalGuesses { get; set; } = 0;

        public int Number1 { get; set; }

        public int Number2 { get; set; }

        public bool CheckAnswer(int answer)
        {
            TotalGuesses++;
            if (answer == (Number1 + Number2))
            {
                CorrectGuesses++;
                return true;
            }
            return false;
        }

        public void NextQuestion()
        {
            Random random = new Random();
            Number1 = random.Next(100);
            Number2 = random.Next(100);
        }
    }

}
