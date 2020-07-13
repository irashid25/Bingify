# Bingify
Creates images for windows spotlight lockscreen wallpapers

# Installation
Ensure that windows Spotlight in running (Locksceen option in windows settings).

1. Clone/download repo.
2. Navigate to the Bingify directory which contains the install.ps1 file
3. Open powershell in admin mode and run the following command: .\install.ps1 -publishDirectory "Path to directory where you want to publish the app - include double back slashes." -serviceName "Service name of your choosing". You will need to enter a valid username(prefixed with the domain) and password of a user with appropriate permissions.
  
Log file is outputted to c:\temp folder.
The images will be copied to your Pictures\Wallpapers folder.
