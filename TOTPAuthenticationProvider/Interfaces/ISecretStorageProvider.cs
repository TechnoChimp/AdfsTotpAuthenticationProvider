namespace AdfsTotpAuthenticationProvider.Interfaces
{
    public interface ISecretStorageProvider
    {
        string GetSecretKey(string upn);
        void SetSecretKey(string upn, string secret);
    }
}