param ($serviceExeFolderLocation, $serviceExeLocation, $domainUserName, $serviceName)
write-host $serviceExeLocation
write-host $domainUserName
write-host $serviceName

$acl = Get-Acl $serviceExeFolderLocation
$aclRuleArgs = $domainUserName, "Read,Write,ReadAndExecute", "None", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "$serviceExeLocation"

New-Service -Name $serviceName -BinaryPathName $serviceExeLocation -Credential $domainUserName -Description "Bingify Service" -DisplayName $serviceName -StartupType Automatic