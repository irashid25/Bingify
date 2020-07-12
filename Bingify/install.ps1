param ($publishDirectory, $serviceName)

Set-ExecutionPolicy RemoteSigned

If (Get-Service $serviceName -ErrorAction SilentlyContinue) {

    If ((Get-Service $serviceName).Status -eq 'Running') {

        Write-Host "Stopping $serviceName"
        sc.exe STOP $ServiceName
        Write-Host "$serviceName stopped"

    } Else {

        Write-Host "$serviceName found, but it is not running."
    }

    Write-Host "deleteing service: $serviceName"
    sc.exe DELETE $ServiceName
    Write-Host "$serviceName deleted"
} 

$MyCredential = Get-Credential
$domainUserName = $MyCredential.UserName
$password = $MyCredential.GetNetworkCredential().password

New-Item -ItemType Directory -Force -Path $publishDirectory
dotnet publish --runtime win-x64 --configuration Release --output $publishDirectory

$acl = Get-Acl $publishDirectory
$aclRuleArgs = $domainUserName, "Read,Write,ReadAndExecute", "None", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "$publishDirectory\Bingify.exe"

$securePassword = convertto-securestring -String $password -AsPlainText -Force  
$credentialsObject = new-object -typename System.Management.Automation.PSCredential -argumentlist $domainUserName, $securePassword


New-Service -Name $serviceName -BinaryPathName "$publishDirectory\Bingify.exe" -Credential $credentialsObject -Description "Bingify Service" -DisplayName $serviceName -StartupType Automatic

Start-Service -Name $serviceName

Get-Service -Name $serviceName