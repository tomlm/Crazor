


namespace Crazor.Interfaces
{
    public interface IEncryptionProvider
    {
        public Task<string> EncryptAsync(string content, CancellationToken cancellationToken);

        public Task<string> DecryptAsync(string encryptedContent, CancellationToken cancellationToken);
    }
}
