# Battleships!!

Tested on Windows
Running one client in PowerShell,
and another client in WSL2 set up according to this guide:
https://www.dotnetthailand.com/programming-cookbook/wsl-powershell-useful-scripts/install-dotnet

Running both on the same OS without virtualisation yields SocketException 10048

Executed using `dotnet run`


> Currently, the two clients connect with a 3s timeout set in Broadcast, but not higher (10s or 30s).
> Ok so they *eventually* connect, but it takes a few minutes... it's much faster with a shorter timeout