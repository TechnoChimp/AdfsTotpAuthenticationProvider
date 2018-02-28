using System;
using System.IO;
using System.Text;
using AdfsTotpAuthenticationProvider.Execeptions;
using AdfsTotpAuthenticationProvider.Interfaces;
using AdfsTotpAuthenticationProvider.Providers;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace AdfsTotpAuthenticationProvider
{
    public class AuthenticationAdapter : IAuthenticationAdapter
    {
        private ActiveDirectorySecretStorageProvider _secretStorageProvider;
        private IUsedCodeProvider _usedCodeProvider;

        public IAdapterPresentation BeginAuthentication(System.Security.Claims.Claim identityClaim, System.Net.HttpListenerRequest request, IAuthenticationContext context)
        {
            IAdapterPresentation result;

            var upn = identityClaim.Value;
            
            var secretKey = _secretStorageProvider.GetSecretKey(upn);

            context.Data.Add("upn", upn);

            if (string.IsNullOrEmpty(secretKey))
            {
                secretKey = TotpAuthenticator.GenerateSecretKey();

                _secretStorageProvider.SetSecretKey(upn, secretKey);

                result = new AdapterPresentation(upn, secretKey);
            }
            else
            {
                result = new AdapterPresentation();
            }

            return result;
        }

        private static string NormalizeUpn(string upn)
        {
            if (upn.Contains("@"))
                return upn.Substring(0, upn.IndexOf("@", StringComparison.Ordinal));

            return upn;
        }

        public bool IsAvailableForUser(System.Security.Claims.Claim identityClaim, IAuthenticationContext context)
        {
            return true;
        }

        public IAuthenticationAdapterMetadata Metadata => new AuthenticationAdapterMetadata();

        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            if (configData?.Data == null)
                throw new NoConfigurationException();

            using (var reader = new StreamReader(configData.Data, Encoding.UTF8))
            {
                var config = reader.ReadToEnd();

                var ldapServer = "";
                var ldapOu = "";
                var ldapSecretField = "info";

                var lines = config.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("LdapServer"))
                        ldapServer = line.Substring(10).Trim();

                    if (line.StartsWith("LdapOu"))
                        ldapOu = line.Substring(6).Trim();

                    if (line.StartsWith("LdapSecretField"))
                        ldapSecretField = line.Substring(15).Trim();
                }

                if (string.IsNullOrWhiteSpace(ldapServer) || string.IsNullOrWhiteSpace(ldapOu) || string.IsNullOrWhiteSpace(ldapSecretField))
                    throw new InvalidConfigurationException();

                _secretStorageProvider = new ActiveDirectorySecretStorageProvider(ldapServer, ldapOu, ldapSecretField);
                _usedCodeProvider = new NullUsedCodeProvider();
            }
        }

        public void OnAuthenticationPipelineUnload()
        {
            // do nothing
        }

        public IAdapterPresentation OnError(System.Net.HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            return new AdapterPresentation();
        }

        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            if (proofData?.Properties == null ||
                !proofData.Properties.ContainsKey("ChallengeQuestionAnswer") ||
                context?.Data == null ||
                !context.Data.ContainsKey("upn") ||
                string.IsNullOrEmpty((string)context.Data["upn"]))
            {
                throw new ExternalAuthenticationException("No answer found or corrupted context.", context);
            }

            claims = null;
            IAdapterPresentation result = null;

            var upn = (string)context.Data["upn"];
            var code = (string)proofData.Properties["ChallengeQuestionAnswer"];

            if (TotpAuthenticator.CheckCode(upn, _secretStorageProvider.GetSecretKey(upn), code, _usedCodeProvider))
            {
                var claim = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                claims = new[] { claim };
            }
            else
            {
                result = new AdapterPresentation();
            }

            return result;
        }
    }
}
