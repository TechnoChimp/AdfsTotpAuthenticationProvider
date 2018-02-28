using AdfsTotpAuthenticationProvider.Interfaces;

namespace AdfsTotpAuthenticationProvider.Providers
{
    class NullUsedCodeProvider : IUsedCodeProvider
    {
        public bool CodeIsUsed(string upn, long interval, int pastIntervals)
        {
            return false;
        }

        public void SetUsedCode(string upn, long interval)
        {
            // do nothing
        }
    }
}
