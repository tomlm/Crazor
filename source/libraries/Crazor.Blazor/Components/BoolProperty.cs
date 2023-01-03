// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Blazor.Components
{
    /// <summary>
    ///  Boolean serializes weird with XML using Blazor.  This property basically allows us to 
    ///  make sure that the bool property is serialized as lowercase string in the XML.
    /// </summary>
    public class BoolProperty
    {
        private bool _value;

        public BoolProperty() { }

        public BoolProperty(bool b)
        {
            _value = b;
        }

        public override string? ToString()
        {
            return _value.ToString().ToLower();
        }

        public static implicit operator BoolProperty(bool val) => new BoolProperty(val);
        public static implicit operator BoolProperty(string val) => new BoolProperty(Convert.ToBoolean(val));
        public static bool operator ==(BoolProperty lhs, BoolProperty rhs) => lhs?._value == rhs?._value;
        public static bool operator !=(BoolProperty lhs, BoolProperty rhs) => lhs?._value != rhs?._value;
    }
}
