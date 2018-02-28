$typeName = "AdfsTotpAuthenticationProvider.AuthenticationAdapter, AdfsTotpAuthenticationProvider, Version=1.0.1.0, Culture=neutral, PublicKeyToken=7ed29f0000dd7766"

[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$publish = New-Object System.EnterpriseServices.Internal.Publish

foreach($file in Get-ChildItem C:\MFA\*.dll) {
    Write-Host $file.FullName
    $publish.GacInstall($file.FullName)
}


Restart-Service adfssrv