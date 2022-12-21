// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace CrazorDemoBot.Cards.ProductCatalog
{
    public class EditOrAddProductCatalogItem
    {
        public bool IsEdit { get; set; }

        public ProductCatalogItem? Item { get; set; }
    }
}
