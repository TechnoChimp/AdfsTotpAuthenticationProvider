using AdfsTotpAuthenticationProvider.Interfaces;

namespace AdfsTotpAuthenticationProvider.Providers
{
    class NullUsedCodeProvider : IUsedCodeProvider
    {
        public bool IsCodeUsed(string upn, long interval)
        {
            return false;
        }

        public void SetUsedCode(string upn, long interval)
        {
            // do nothing
        }
    }
}
