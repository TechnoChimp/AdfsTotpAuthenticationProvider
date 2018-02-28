using System;
using AdfsTotpAuthenticationProvider.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace AdfsTotpAuthenticationProvider.Providers
{
    public class DynamoDbUsedCodeProvider : IUsedCodeProvider
    {
        private readonly Table _table;
        private readonly AmazonDynamoDBClient _client = new AmazonDynamoDBClient();

        public DynamoDbUsedCodeProvider()
        {
            _table = Table.LoadTable(_client, "adfs-mfa-tokens");
        }

        public bool IsCodeUsed(string upn, long interval)
        {
            var item = _table.GetItem(upn, interval);

            return item != null;
        }

        public void SetUsedCode(string upn, long interval)
        {
            var item = new Document
            {
                {"upn", upn},
                {"interval", interval},
                {"expires", ToUnixTime(DateTime.UtcNow.AddMinutes(10)) },
            };

            _table.PutItem(item);
        }

        private static long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
    }
}
