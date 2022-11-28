// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    /// <summary>
    /// This property will be persisted scoped to the value of a property on the same class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PathMemoryAttribute : MemoryAttribute
    {
        public PathMemoryAttribute(string propertyPath)
        {
            this.PropertyPath = propertyPath;
        }

        public string PropertyPath { get; set; }

        public override string? GetKey(object obj)
        {
            if (ObjectPath.TryGetPathValue<string>(obj, PropertyPath, out var result))
                return $"{PropertyPath.Replace(".", String.Empty)}={result}";
            return null;
        }
    }
}
