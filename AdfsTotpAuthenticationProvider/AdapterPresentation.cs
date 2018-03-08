using Microsoft.IdentityServer.Web.Authentication.External;

namespace AdfsTotpAuthenticationProvider
{
    internal class AdapterPresentation : IAdapterPresentationForm
    {
        private readonly string _upn;
        private readonly SecretKey _secretKey;

        public AdapterPresentation(string upn = null, SecretKey secretKey = null)
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

            if (_secretKey == null || _secretKey.Activated)
            {
                htmlTemplate = htmlTemplate.Replace("PICTUREHERE", "");
            }
            else
            {
                const int width = 150;
                const int height = 150;

                const string text = "<p>Please configure your Authenticator App (Authy, Google Authenticator, Microsoft Verificator or other) using the QR code below.</p><br />";

                htmlTemplate = htmlTemplate.Replace("PICTUREHERE", text + string.Format("<img width=\"{0}\" height=\"{1}\" src=\"https://chart.googleapis.com/chart?chs={0}x{1}&amp;chld=M%7C0&amp;cht=qr&amp;chl=otpauth%3A%2F%2Ftotp%2F{2}%3Fsecret%3D{3}\">", width, height, _upn, _secretKey.Key));
            }

            return htmlTemplate;
        }

        public string GetFormPreRenderHtml(int lcid)
        {
            return string.Empty;
        }
    }
}
