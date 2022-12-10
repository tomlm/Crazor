// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Crazor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Crazor.Mvc
{
    internal interface IMvcCardView : ICardView
    {

        IView RazorView { get; set; }

        /// <summary>
        /// UrlHelper for creating links to resources on this service.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

    }
}
