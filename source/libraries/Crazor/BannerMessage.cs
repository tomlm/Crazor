using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Crazor
{
    public class BannerMessage
    {
        public string Text { get; set; } = String.Empty;

        public AdaptiveContainerStyle Style { get; set; } = AdaptiveContainerStyle.Accent;
    }
}
