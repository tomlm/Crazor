namespace Crazor.Attributes
{

    /// <summary>
    /// TaskInfo attribute is an attribute which controls the title, width and height of a task module.
    /// </summary>
    /// <remarks>
    /// This comes into play when a view has a [CommandInfo] defined saying that view should be hosted in a L2 TaskModule.
    /// * When this is placed on an CardApp this controls the metadata for all views
    /// * When this is placed on a CardView this controls the metadata for this view.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
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

        /// <summary>
        /// The title to use for the taskmodule window
        /// </summary>
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
