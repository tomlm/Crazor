using Microsoft.Bot.Schema;

namespace SharedCards.Cards.Wordle
{
    public class WordleGame
    {
        public WordleGame(string word, DateTimeOffset date)
        {
            Word = word;
            Date = date;
        }

        public string Word { get; set; }

        public ChannelAccount Player { get; set; }

        public DateTimeOffset Date { get; set; }

        public bool Won { get; set; } = false;

        public bool IsDone => Won == true || Guesses.Count == 6;

        public List<Guess> Guesses { get; set; } = new List<Guess>();

        public string GetResultsRoute() => $"{Date.ToString("yyyyMMdd")}/{WordleApp.Sanitize(Player.Id)}";

        public void AddUserGuess(string guess)
        {
            if (Guesses.Count < 6)
            {
                if (guess.Length == 5 && !Guesses.Any(g => g.Value == guess))
                    Guesses.Add(new Guess(guess, Word));

                if (guess.ToUpper() == Word)
                    Won = true;
            }
        }
    }
}
