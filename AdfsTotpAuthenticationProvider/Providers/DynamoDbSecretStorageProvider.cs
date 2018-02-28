using AdfsTotpAuthenticationProvider.Interfaces;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace AdfsTotpAuthenticationProvider.Providers
{
    class DynamoDbSecretStorageProvider : ISecretStorageProvider
    {
        private readonly Table _table;
        private readonly AmazonDynamoDBClient _client = new AmazonDynamoDBClient();

        public DynamoDbSecretStorageProvider()
        {
            _table = Table.LoadTable(_client, "adfs-mfa-secrets");
        }

        public string GetSecretKey(string upn)
        {
            var item = _table.GetItem(upn);

            if (item == null)
                return null;

            return item["secret"];
        }

        public void SetSecretKey(string upn, string secret)
        {
            var item = _table.GetItem(upn) ?? new Document
            {
                ["upn"] = upn
            };

            item["secret"] = secret;

            _table.PutItem(item);
        }
    }
}