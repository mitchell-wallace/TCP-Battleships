# Battleships!!

Tested on Windows 11 build 22621 with .NET 6.0.401,
Running one instance in PowerShell 7,
and another instance in WSL2 with Ubuntu-20.04 set up according to this guide:
https://www.dotnetthailand.com/programming-cookbook/wsl-powershell-useful-scripts/install-dotnet
using localhost:5001 for both instances
Visual studio is not necessary, thought it does work

The PowerShell instance and the WSL instance will need to be pointed to different folders containing the same source code

Running both on the same OS without virtualisation yields SocketException 10048
It's probably possible to fix this but I didn't find a solution in a reasonable space of time

Executed using `dotnet run`

All features working on my system, though the WSL instance always seems to end up as Player #1
even though they run the same code

After initial connection is established, player #1 will have to wait for the 30 second timer
on UDP listening to expire before the program resumes

## Copy of WSL .NET install commands
  `wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb`
  `sudo dpkg -i packages-microsoft-prod.deb`
  `rm packages-microsoft-prod.deb`

  `sudo apt update; \
    sudo apt install -y apt-transport-https && \
    sudo apt update && \
    sudo apt install -y dotnet-sdk-6.0`