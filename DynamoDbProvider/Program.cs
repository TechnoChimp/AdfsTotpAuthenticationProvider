using System;
using AdfsTotpAuthenticationProvider.Providers;

namespace DynamoDbProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            var upn = "test2";

            var provider = new DynamoDbUsedCodeProvider();

            provider.SetUsedCode(upn, 10);
            provider.SetUsedCode(upn, 12);

            Console.WriteLine(provider.IsCodeUsed(upn, 10));
            Console.WriteLine(provider.IsCodeUsed(upn, 11));
            Console.WriteLine(provider.IsCodeUsed(upn, 12));
            Console.WriteLine(provider.IsCodeUsed(upn, 13));

            Console.ReadLine();
        }
    }
}