using System.IO;

namespace AdfsTotpAuthenticationProvider
{
    public static class Logger
    {
        public static void Log(string text)
        {
            using (var sw = new StreamWriter(@"C:\MFA\logs.txt", true))
            {
                sw.WriteLine(text);
            }
        }
    }
}
