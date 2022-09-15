//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Cards.Interfaces;

namespace Microsoft.Bot.Cards.Encryption
{
    /// <summary>
    /// EncryptionProvider which doesn't encrypt the data...
    /// </summary>
    public class NoEncryptionProvider : IEncryptionProvider
    {
        public NoEncryptionProvider()
        {
        }

        public Task<string> DecryptAsync(string encryptedContent, CancellationToken cancellationToken)
        {
            return Task.FromResult(encryptedContent);
        }

        public Task<string> EncryptAsync(string content, CancellationToken cancellationToken)
        {
            return Task.FromResult(content);
        }
    }
}