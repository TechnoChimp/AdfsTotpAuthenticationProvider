$typeName = "AdfsTotpAuthenticationProvider.AuthenticationAdapter, AdfsTotpAuthenticationProvider, Version=1.0.1.0, Culture=neutral, PublicKeyToken=7ed29f0000dd7766"

Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider {}
Unregister-AdfsAuthenticationProvider -Name AdfsTotpAuthenticationProvider -confirm:$false

Restart-Service adfssrv

gacutil.exe /u $typeName