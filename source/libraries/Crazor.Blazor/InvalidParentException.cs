using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Blazor
{
    public class InvalidParentException : Exception
    {
        public InvalidParentException(Type parentType, Type childType) : base($"{parentType.Name} is not valid parent for {childType.Name}")
        {
        }

        public Type ParentType { get; set; }

        public Type ChildType { get; set; }

    }
}
