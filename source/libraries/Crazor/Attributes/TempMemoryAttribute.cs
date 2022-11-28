// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{

    /// <summary>
    /// This property will not be persisted at all.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TempMemoryAttribute : MemoryAttribute
    {
        public override string? GetKey(object obj)
        {
            return null;
        }
    }
}
