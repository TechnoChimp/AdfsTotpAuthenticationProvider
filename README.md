# AdfsTotpAuthenticationProvider

This project implements TOTP authentication provider for Active Directory Federation Services. By using this provider you can add MFA for your ADFS and enable users to authenticate using Google Authenticator, Microsoft Verifier, Authy and similar applications.

## Backend

At the beginning I've implemented storage backend based on Active Directory but it didn't work well in multi-site scenario as replication delay caused some issues.
So I've implemented another backend based on DynamoDB. The backend stores secrets and tokens (to prevent multiple usage of one ticket) with auto-delete of old tokens.

You can create required DynamoDB tables for example using Terraform:

```
resource "aws_dynamodb_table" "adfs_mfa_secrets" {
  name = "adfs-mfa-secrets"
  read_capacity = 1
  write_capacity = 1
  hash_key = "upn"

  attribute {
    name = "upn"
    type = "S"
  }
}

resource "aws_dynamodb_table" "adfs_mfa_tokens" {
  name = "adfs-mfa-tokens"
  read_capacity = 1
  write_capacity = 1
  hash_key = "upn"
  range_key = "interval"

  ttl {
    attribute_name = "expires"
    enabled = true
  }

  attribute {
    name = "upn"
    type = "S"
  }

  attribute {
    name = "interval"
    type = "N"
  }
}
```

## Installation

1. Copy compiled DLL files to C:\MFA on each ADFS server
2. Create necessary tables in DynamoDB
3. Run RegisterPrimary.ps1 script on primary server
4. Run RegisterSecondary.ps1 script on all secondary servers
5. Enforce MFA on selected relying party trusts
6. Sign in to ADFS and login to selected relying party trust
7. The token will be generated, scan it with Google Authenticator or similar app
8. Enjoy :)

## Credits

The project is based on the code presented in [the article](https://blogs.technet.microsoft.com/cloudpfe/2014/10/26/using-time-based-one-time-passwords-for-multi-factor-authentication-in-ad-fs-3-0/).