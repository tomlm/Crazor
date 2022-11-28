// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    /// <summary>
    /// This property will be persisted scoped to the current interaction session
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SessionMemoryAttribute : PathMemoryAttribute
    {
        public SessionMemoryAttribute() : base("Route.SessionId")
        {
        }
    }
}
