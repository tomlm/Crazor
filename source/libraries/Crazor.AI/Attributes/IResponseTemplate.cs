// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

using Crazor.Attributes;

namespace Crazor.AI.Attributes
{

    public interface IResponseTemplate
    {
        string BindTemplate(IDictionary<string, object> data);
    }
}
