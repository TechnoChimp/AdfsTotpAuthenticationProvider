namespace AdfsTotpAuthenticationProvider.Interfaces
{
    public interface IUsedCodeProvider
    {
        bool IsCodeUsed(string upn, long interval);
        void SetUsedCode(string upn, long interval);
    }
}