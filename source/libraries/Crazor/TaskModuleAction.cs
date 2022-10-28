namespace Crazor
{
    public enum TaskModuleAction
    {
        /// <summary>
        /// Card is still being worked on
        /// </summary>
        Continue, 

        /// <summary>
        /// Done, don't share a card
        /// </summary>
        None,

        /// <summary>
        /// Done, Insert a card into the compose window
        /// </summary>
        InsertCard,

        /// <summary>
        /// Done, Post a message to the channel with a card embedded in it
        /// </summary>
        PostCard,

        /// <summary>
        /// Done, Post if it's a commandbox, insert if it's a compose box.
        /// </summary>
        Auto
    }
}
