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

        public SecretKey GetSecretKey(string upn)
        {
            var item = _table.GetItem(upn);

            if (item == null)
                return null;

            return new SecretKey()
            {
                Key = item["secret"],
                Activated = item["activated"] == "true"
            };
        }

        public void SetSecretKey(string upn, SecretKey secret)
        {
            var item = _table.GetItem(upn) ?? new Document
            {
                ["upn"] = upn
            };

            item["secret"] = secret.Key;
            item["activated"] = secret.Activated ? "true" : "false";

            _table.PutItem(item);
        }
    }
}