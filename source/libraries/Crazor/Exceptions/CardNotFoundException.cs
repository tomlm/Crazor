// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Exceptions
{
    /// <summary>
    /// Throw this exception if the LoadRoute fails to bind.
    /// </summary>
    public class CardRouteNotFoundException : Exception
    {
        public CardRouteNotFoundException(string message) : base(message ?? "Card not found") 
        { 
        }
    }
}
