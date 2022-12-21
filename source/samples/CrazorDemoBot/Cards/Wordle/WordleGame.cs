// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace CrazorDemoBot.Cards.Wordle
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

        public bool MakeGuess(string guess)
        {
            if (guess.Length == 5 && !Guesses.Any(g => g.Value == guess))
                Guesses.Add(new Guess(guess, Word));

            return guess.ToUpper() == Word;
        }
    }
}
