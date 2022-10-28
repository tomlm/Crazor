using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor
{
    public enum TaskModuleStatus
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
        Insert,
        
        /// <summary>
        /// Done, Post a message to the channel with a card embedded in it
        /// </summary>
        Post
    }
}
