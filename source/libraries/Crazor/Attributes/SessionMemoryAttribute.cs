﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crazor.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public class SessionMemoryAttribute : PropertyValueMemoryAttribute
    {
        public SessionMemoryAttribute() : base("SessionId")
        {
        }
    }
}
