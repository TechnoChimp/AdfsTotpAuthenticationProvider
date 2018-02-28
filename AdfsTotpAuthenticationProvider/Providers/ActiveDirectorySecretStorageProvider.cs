using System;
using System.DirectoryServices;
using AdfsTotpAuthenticationProvider.Execeptions;
using AdfsTotpAuthenticationProvider.Interfaces;

namespace AdfsTotpAuthenticationProvider.Providers
{
    class ActiveDirectorySecretStorageProvider : ISecretStorageProvider
    {
        private readonly string _ldapServer;
        private readonly string _ldapOu;
        private readonly string _ldapSecretField;

        public static ActiveDirectorySecretStorageProvider CreateFromConfig(string config)
        {
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

            return new ActiveDirectorySecretStorageProvider(ldapServer, ldapOu, ldapSecretField);
        }

        public ActiveDirectorySecretStorageProvider(string ldapServer, string ldapOu, string ldapSecretField = "info")
        {
            _ldapServer = ldapServer;
            _ldapOu = ldapOu;
            _ldapSecretField = ldapSecretField;
        }

        public string GetSecretKey(string upn)
        {
            var user = SearchUser(upn);

            if (user == null)
                throw new UserNotFoundException();

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
                Filter = "(userPrincipalName=" + upn + ")"
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
