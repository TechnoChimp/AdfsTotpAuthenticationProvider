using System.Collections.Generic;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace AdfsTotpAuthenticationProvider
{
    internal class AuthenticationAdapterMetadata : IAuthenticationAdapterMetadata
    {
        public string AdminName => "AdfsTotpAuthenticationProvider";
        public string[] AuthenticationMethods => new[] { "http://schemas.microsoft.com/ws/2012/12/authmethod/otp" };

        public int[] AvailableLcids => new[] { 1033 };

        public Dictionary<int, string> Descriptions
        {
            get
            {
                var result = new Dictionary<int, string>();
                result.Add(1033, "AdfsTotpAuthenticationProvider");
                return result;
            }
        }

        public Dictionary<int, string> FriendlyNames
        {
            get
            {
                var result = new Dictionary<int, string>();
                result.Add(1033, "AdfsTotpAuthenticationProvider");
                return result;
            }
        }

        public string[] IdentityClaims => new[] { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" };

        public bool RequiresIdentity => true;
    }
}
