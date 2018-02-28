namespace AdfsTotpAuthenticationProvider.Interfaces
{
    public interface IUsedCodeProvider
    {
        bool CodeIsUsed(string upn, long interval, int pastIntervals);
        void SetUsedCode(string upn, long interval);
    }
}