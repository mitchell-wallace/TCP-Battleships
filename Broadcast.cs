using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Battleships
{
    internal class Broadcast
    {
        private static UdpClient? udpClient;
        private static IPEndPoint? udpEndPoint;
        private static UdpState? udpState;
        public static bool contacted = false; // indicates whether a broadcast or TCP message has been received
        private static TimeSpan ttw = TimeSpan.FromSeconds(30); // time to wait between broadcasts
        public static void Connect()
            // overall logic for using broadcasts to establish tcp connection
        {
            udpClient = new UdpClient(Battleships.BcPort);
            udpEndPoint = new IPEndPoint(Battleships.BcAddress, Battleships.BcPort);
            udpState = new UdpState(udpClient, udpEndPoint);

            Thread tcpListen = new Thread(new ThreadStart(OpponentConnection.InitialListen));
            tcpListen.Start();

            while (!contacted)
            {
                Listen();
                if (!contacted) Send();
            }
        }

        private static void Listen()
            // listen for a broadcast from another instance
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");
            Console.ResetColor();

            if (Battleships.PlayerNo != 0) return; // if we have been assigned a player number,
                // this means an async result has been processed, so we don't want to keep processing!

            var asyncResult = udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

            Console.WriteLine("Listening.....");
            asyncResult.AsyncWaitHandle.WaitOne(ttw);
            
            // This block is for udpClient cleanup; response is actually proecessed in ReceiveCallback();
            if (asyncResult.IsCompleted)
            {
                try
                {
                    // yes, remoteEP is null, and it's okay for it to be null.
                    IPEndPoint? remoteEP = null;
                    udpClient.EndReceive(asyncResult, ref remoteEP!);
                }
                catch (Exception e)
                {
                    // EndReceive failed; print exception and keep trying
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Error receiving UDP message:\n{e}");
                    Console.ResetColor();
                    Thread.Sleep(1000);   
                }
            }
        }

        private static void Send()
            // broadcast a UDP message
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");
            Console.ResetColor();

            try
            {
                Console.WriteLine("Broadcasting...");
                var data = Encoding.UTF8.GetBytes($"NEW PLAYER:{Battleships.RandomTcpPort}");
                udpClient.Send(data, data.Length, "255.255.255.255", Battleships.BcPort);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"An exception occurred while attempting to broadcast a UDP packet;\n{e}");
                Console.ResetColor();
                Thread.Sleep(1000);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
            // process the callback and initiate TCP connection process
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");
            Console.ResetColor();

            byte[] receiveBytes = udpClient.EndReceive(ar, ref udpEndPoint);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            if (receiveString != $"NEW PLAYER:{Battleships.RandomTcpPort}" // ignore messages broadcast from this client
                && Battleships.PlayerNo == 0) // if player number already assigned, TCP connection already started
            {
                contacted = true;
                Battleships.PlayerNo = 2;
                Battleships.AgreedTcpPort = int.Parse(receiveString[11..]); // "NEW PLAYER:".Length = 11; we extract port only

                // Initiate TCP connection as player 2, or "client"
                OpponentConnection.InitiateAsClient();
            }
        }
    }

    internal class UdpState
        // can we replace the UdpState object with our static variables?
        // no, we need it for setting up the asyncResult
    {
        public UdpClient? u;
        public IPEndPoint? e;
        public UdpState(UdpClient? u, IPEndPoint? e)
        {
            this.u = u;
            this.e = e;
        }
    }
}