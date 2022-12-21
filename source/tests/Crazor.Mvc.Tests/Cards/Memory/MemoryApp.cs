// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.Mvc.Tests.Cards.Memory
{
    public class MemoryApp : CardApp
    {
        public MemoryApp(CardAppContext context) : base(context)
        {
        }

        [AppMemory]
        public string? App { get; set; }

        [SessionMemory]
        public string? Session { get; set; }

        [UserMemory]
        public string? User { get; set; }

        [ConversationMemory]
        public string? Conversation { get; set; }

        [PathMemory("Value")]
        public string? Path { get; set; }

        [TempMemory]
        public string? Temp { get; set; }

        public string Value { get; set; } = "Value";
    }
}