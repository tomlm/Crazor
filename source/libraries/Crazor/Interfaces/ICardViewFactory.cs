// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Crazor.Interfaces
{
    public interface ICardViewFactory
    {
        void Add(string name, Type type);

        IEnumerable<string> GetNames();

        ICardView Create(CardRoute route);

        ICardView Create(string fullTypeName);
    }
}
