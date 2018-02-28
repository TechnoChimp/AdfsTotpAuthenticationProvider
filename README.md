1. Copy DLL file to C:\MFA
2. Create config file C:\MFA\config.txt
3. Grant ADFS service account ability to read and write users attributes (required to generate new tokens)
4. Run RegisterPrimary.ps1 script on primary server
5. Run RegisterSecondary.ps1 script on all secondary servers
6. Enforce MFA on selected relying party trusts
7. Sign in to ADFS and login to selected relying party trust
8. The token will be generated, scan it with Google Authenticator or similar app
9. Enjoy :)