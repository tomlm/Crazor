


using Crazor.Interfaces;

namespace Crazor.Encryption
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