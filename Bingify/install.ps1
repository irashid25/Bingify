param ($serviceExeFolderLocation, $serviceName)

$MyCredential = Get-Credential

$domainUserName = $MyCredential.UserName

$password = $MyCredential.GetNetworkCredential().password

Write-Host $domainUserName
Write-Host $password



$acl = Get-Acl $serviceExeFolderLocation
$aclRuleArgs = $domainUserName, "Read,Write,ReadAndExecute", "None", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "$serviceExeFolderLocation\Bingify.exe"

$securePassword = convertto-securestring -String $password -AsPlainText -Force  
$credentialsObject = new-object -typename System.Management.Automation.PSCredential -argumentlist $domainUserName, $securePassword


New-Service -Name $serviceName -BinaryPathName "$serviceExeFolderLocation\Bingify.exe" -Credential $credentialsObject -Description "Bingify Service" -DisplayName $serviceName -StartupType Automatic

Start-Service -Name $serviceName

Get-Service -Name $serviceName