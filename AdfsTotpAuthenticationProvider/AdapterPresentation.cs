using System;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace AdfsTotpAuthenticationProvider
{
    internal class AdapterPresentation : IAdapterPresentationForm
    {
        private readonly string _upn;
        private readonly string _secretKey;

        public AdapterPresentation(string upn = null, string secretKey = null)
        {
            _upn = upn;
            _secretKey = secretKey;
        }

        public string GetPageTitle(int lcid)
        {
            return "TOTP Authentication Provider";
        }

        public string GetFormHtml(int lcid)
        {
            var htmlTemplate = Resources.AuthenticationForm;

            if (string.IsNullOrEmpty(_secretKey))
            {
                htmlTemplate = htmlTemplate.Replace("PICTUREHERE", "");
            }
            else
            {
                var width = 150;
                var height = 150;

                var text = "<p>Please configure your Authenticator App (Authy, Google Authenticator, Microsoft Verificator or other) using the QR code below.</p><br />";

                htmlTemplate = htmlTemplate.Replace("PICTUREHERE", text + string.Format("<img width=\"{0}\" height=\"{1}\" src=\"https://chart.googleapis.com/chart?chs={0}x{1}&amp;chld=M%7C0&amp;cht=qr&amp;chl=otpauth%3A%2F%2Ftotp%2F{2}%3Fsecret%3D{3}\">", width, height, this._upn, this._secretKey));
            }

            return htmlTemplate;
        }

        public string GetFormPreRenderHtml(int lcid)
        {
            return string.Empty;
        }
    }
}
