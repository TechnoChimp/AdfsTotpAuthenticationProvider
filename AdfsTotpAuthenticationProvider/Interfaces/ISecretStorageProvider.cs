namespace AdfsTotpAuthenticationProvider.Interfaces
{
    public interface ISecretStorageProvider
    {
        SecretKey GetSecretKey(string upn);
        void SetSecretKey(string upn, SecretKey secret);
    }
}