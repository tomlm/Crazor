// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.AdaptiveCards;

namespace CrazorDemoBot.Cards.Wordle
{
    public class GuessLetter
    {
        public Char Value { get; set; }

        public AdaptiveContainerStyle State { get; set; }
    }

    public class Guess
    {
        public Guess()
        { }

        public Guess(string guess, string word)
        {
            guess = guess.ToUpper();
            this.Value = guess;

            for (int i = 0; i < 5; i++)
            {
                Letters[i] = new GuessLetter()
                {
                    Value = guess[i],
                    State = AdaptiveContainerStyle.Emphasis
                };

                if (word[i] == guess[i])
                    Letters[i].State = AdaptiveContainerStyle.Good;
            }

            for (int i = 0; i < 5; i++)
            {
                var goodCount = Letters.Count(letter => letter.State == AdaptiveContainerStyle.Good && letter.Value == guess[i]);
                var letterCount = word.Count(letter => letter == guess[i]);

                if (Letters[i].State != AdaptiveContainerStyle.Good && goodCount < letterCount)
                {
                    Letters[i].State = AdaptiveContainerStyle.Warning;
                }
            }
        }

        public string Value { get; set; }

        public GuessLetter[] Letters { get; set; } = new GuessLetter[5];
    }
}