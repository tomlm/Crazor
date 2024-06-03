using Crazor;
using Crazor.Attributes;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCards.Cards.UnitTest
{
    public class UnitTestApp : CardApp
    {
        public UnitTestApp(CardAppContext context) : base(context)
        {
        }

        [SessionMemory]
        public InsertionType InsertionType { get; set; } = InsertionType.Unknown;

        [SessionMemory]
        public AppHost AppHost { get; set; } = AppHost.Unknown;
    }
}
