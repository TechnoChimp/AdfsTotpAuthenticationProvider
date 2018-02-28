1. Copy compiled DLL files to C:\MFA
2. Create necessary tables in DynamoDB
3. Run RegisterPrimary.ps1 script on primary server
4. Run RegisterSecondary.ps1 script on all secondary servers
5. Enforce MFA on selected relying party trusts
6. Sign in to ADFS and login to selected relying party trust
7. The token will be generated, scan it with Google Authenticator or similar app
8. Enjoy :)