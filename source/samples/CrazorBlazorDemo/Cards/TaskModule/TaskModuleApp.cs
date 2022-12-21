// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor;
using Crazor.Attributes;

namespace CrazorBlazorDemo.Cards.TaskModule
{
    [TaskInfo(Width = "small", Height = "medium", Title = "Test the Task Module")]
    public class TaskModuleApp : CardApp
    {
        public TaskModuleApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public int Counter { get; set; }
    }
}
