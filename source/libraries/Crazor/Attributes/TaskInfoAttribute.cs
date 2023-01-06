// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TaskInfoAttribute : Attribute
    {
        public TaskInfoAttribute()
        {
        }

        public TaskInfoAttribute(string? Title = null, string? Width = null, string? Height = null)
        {
            if (Title != null)
                this.Title = Title;

            if (Width != null)
                this.Width = Width;
            if (Height != null)
                this.Height = Height;
        }

        public string? Title { get; set; }

        /// <summary>
        /// Value is [small, medium, large] or an integer which is px
        /// </summary>
        public string Width { get; set; } = "medium";

        /// <summary>
        /// Value is [small, medium, large] or an integer which is px
        /// </summary>
        public string Height { get; set; } = "medium";
    }
}
