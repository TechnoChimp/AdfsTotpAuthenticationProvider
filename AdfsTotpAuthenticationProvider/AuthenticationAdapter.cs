using System;
using AdfsTotpAuthenticationProvider.Interfaces;
using AdfsTotpAuthenticationProvider.Providers;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace AdfsTotpAuthenticationProvider
{
    public class AuthenticationAdapter : IAuthenticationAdapter
    {
        private ISecretStorageProvider _secretStorageProvider;
        private IUsedCodeProvider _usedCodeProvider;

        public IAdapterPresentation BeginAuthentication(System.Security.Claims.Claim identityClaim, System.Net.HttpListenerRequest request, IAuthenticationContext context)
        {
            var upn = identityClaim.Value;

            var secretKey = _secretStorageProvider.GetSecretKey(upn);

            context.Data.Add("upn", upn);

            if (secretKey == null)
            {
                secretKey = new SecretKey
                {
                    Key = TotpAuthenticator.GenerateSecretKey(),
                    Activated = false
                };

                _secretStorageProvider.SetSecretKey(upn, secretKey);
            }

            return new AdapterPresentation(upn, secretKey);
        }

        public bool IsAvailableForUser(System.Security.Claims.Claim identityClaim, IAuthenticationContext context)
        {
            return true;
        }

        public IAuthenticationAdapterMetadata Metadata => new AuthenticationAdapterMetadata();

        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            try
            {
                _secretStorageProvider = new DynamoDbSecretStorageProvider();
                _usedCodeProvider = new DynamoDbUsedCodeProvider();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                Logger.Log(ex.StackTrace);
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

            var secretKey = _secretStorageProvider.GetSecretKey(upn);

            if (TotpAuthenticator.CheckCode(upn, secretKey.Key, code, _usedCodeProvider))
            {
                var claim = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");

                claims = new[] { claim };

                if (!secretKey.Activated)
                {
                    secretKey.Activated = true;

                    _secretStorageProvider.SetSecretKey(upn, secretKey);
                }
            }
            else
            {
                result = new AdapterPresentation();
            }

            return result;
        }
    }
}