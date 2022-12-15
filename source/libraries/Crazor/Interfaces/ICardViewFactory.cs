// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Interfaces
{
    public interface ICardViewFactory
    {
        ICardView Create(Type cardViewType);
    }
}
