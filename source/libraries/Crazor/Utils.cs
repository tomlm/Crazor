// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Crazor
{
    public static class Utils
    {
        private readonly static char[] s_base62 = new char[]
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '0','1','2','3','4','5','6','7','8','9'
        };

        /// <summary>
        /// Create a random unique id
        /// </summary>
        /// <param name="size">number of chars to generate</param>
        /// <returns>unique string</returns>
        /// <remarks>
        /// 5 chars will generate a unique id with 62^4*10^1=        147,763,360 combos
        /// 6 chars will generate a unique id with 62^4*10^2=      1,477,633,600 combos
        /// 7 chars will generate a unique id with 62^5*10^2=     91,613,283,200 combos
        /// Exmaple Ids:
        ///     p1fN9J
        ///     v3Vz1E
        ///     L5cf6H
        ///     B8XO9d
        /// We inject number every 2 chars to break up "words" that can be generated from the letters.
        /// </remarks>
        public static string GetNewId(int size = 6)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                if (i % 3 == 2)
                    sb.Append(rnd.Next(10).ToString("x"));
                else
                    sb.Append(s_base62[rnd.Next(s_base62.Length)]);
            }
            return sb.ToString();
        }

        public static AdaptiveCardInvokeValue TransfromSubmitDataToExecuteAction(JObject data)
        {
            // reconstruct actionexecute from .data[Action.Execute]
            // gin up a AdaptiveCardInvokeValue
            dynamic d = data;
            var invokeValue = new AdaptiveCardInvokeValue()
            {
                Action = new AdaptiveCardInvokeAction()
                {
                    Data = data,
                    Id = (string)d._id,
                    Verb = (string)d._verb
                }
            };

            // copy data over (skipping Action.Execute property)
            foreach (var property in data.Properties().Where(property => property.Name != AdaptiveExecuteAction.TypeName))
            {
                ((JObject)invokeValue.Action.Data)[property.Name] = property.Value;
            }

            return invokeValue;
        }




    }
}
