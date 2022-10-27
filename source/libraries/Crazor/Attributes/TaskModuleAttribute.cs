using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TaskModuleAttribute : Attribute
    {
        public TaskModuleAttribute()
        {
        }

        public TaskModuleAttribute(string? Title = null, string? Width = null, string? Height = null)
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
        public string Width { get; set; } = "small";

        /// <summary>
        /// Value is [small, medium, large] or an integer which is px
        /// </summary>
        public string Height { get; set; } = "small";

        public TaskModuleTaskInfo AsTaskInfo(string title)
        {
            object height = int.TryParse(this.Height, out int h)  ? h: this.Height;
            object width = int.TryParse(this.Width, out int w) ? w : this.Width;

            return new TaskModuleTaskInfo()
            {
                Height = height,
                Width = width,
                Title = this.Title ?? title ?? String.Empty
            };
        }
    }
}
