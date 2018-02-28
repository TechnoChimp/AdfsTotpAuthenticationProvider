using System.DirectoryServices;
using AdfsTotpAuthenticationProvider.Interfaces;

namespace AdfsTotpAuthenticationProvider.Providers
{
    class ActiveDirectorySecretStorageProvider : ISecretStorageProvider
    {
        private readonly string _ldapServer;
        private readonly string _ldapOu;
        private readonly string _ldapSecretField;

        public ActiveDirectorySecretStorageProvider(string ldapServer, string ldapOu, string ldapSecretField = "info")
        {
            _ldapServer = ldapServer;
            _ldapOu = ldapOu;
            _ldapSecretField = ldapSecretField;
        }

        public string GetSecretKey(string upn)
        {
            var user = SearchUser(upn);

            return user.GetDirectoryEntry().Properties[_ldapSecretField].Value as string;
        }

        public void SetSecretKey(string upn, string secret)
        {
            var user = SearchUser(upn);
            var entry = user.GetDirectoryEntry();

            entry.Properties[_ldapSecretField].Value = secret;
            entry.CommitChanges();
        }

        private SearchResult SearchUser(string upn)
        {
            var myLdapConnection = CreateDirectoryEntry();

            var search = new DirectorySearcher(myLdapConnection)
            {
                Filter = "(cn=" + upn + ")"
            };

            return search.FindOne();
        }

        private DirectoryEntry CreateDirectoryEntry()
        {
            var ldapConnection = new DirectoryEntry(_ldapServer)
            {
                Path = $"LDAP://{_ldapOu}",
                AuthenticationType = AuthenticationTypes.Secure
            };

            return ldapConnection;
        }
    }
}
