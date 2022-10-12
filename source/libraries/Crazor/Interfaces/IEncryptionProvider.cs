//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Cards.Interfaces
{
    public interface IEncryptionProvider
    {
        public Task<string> EncryptAsync(string content, CancellationToken cancellationToken);

        public Task<string> DecryptAsync(string encryptedContent, CancellationToken cancellationToken);
    }
}
