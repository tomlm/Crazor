// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.Mvc.Tests.Cards.TaskModule
{
    [TaskInfo(Width = "small", Height = "medium", Title = "Test Task Module")]
    public class TaskModuleApp : CardApp
    {
        public TaskModuleApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public int Counter { get; set; }
    }
}
