using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleships
{
    internal class Broadcast
    {
        private static UdpClient? udpClient;
        private static IPEndPoint? udpEndPoint;
        private static UdpState? udpState;
        private static bool messageReceived = false;
        private static bool contacted = false; // indicates whether a broadcast message has been received
        private static bool firstTry = true; // print extra debug info until first execution of Send()
        private static TimeSpan ttw = TimeSpan.FromSeconds(6);
        static byte[] bytes = new byte[2048]; // for listen2 and receive2
        public static void Connect()
        {
            udpClient = new UdpClient(Battleships.BcPort);
            udpEndPoint = new IPEndPoint(Battleships.BcAddress, Battleships.BcPort);
            udpState = new UdpState(udpClient, udpEndPoint);

            // manually set some properties because we want to be sure they're set correctly
            // actually I think this breaks stuff.........
            // udpClient = new UdpClient();
            // udpClient.EnableBroadcast = true;
            // udpClient.Ttl = 96;
            // udpClient.ExclusiveAddressUse = false;
            // udpClient.Client.Bind(udpEndPoint);

            Thread tcpListen = new Thread(new ThreadStart(OpponentConnection.ListenAsHost));

            while (!contacted)
            {
                Listen();
                if (!contacted) Send();
            }
            // we then have to send a TCP message
            // this means we also have to already be listening for a TCP message??
        }

        private static void Listen() // we are trying to do BeginReceive and EndReceive here
        {
            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");

            if (Battleships.PlayerNo != 0) return;

            if (firstTry) Console.WriteLine("Looking for existing game host...");
            var asyncResult = udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

            Console.WriteLine("\nListening.....");
            asyncResult.AsyncWaitHandle.WaitOne(ttw);
            if (asyncResult.IsCompleted)
            {
                try
                {
                    // yes, remoteEP is null, and it's okay for it to be null.
                    IPEndPoint? remoteEP = null;
                    byte[] receivedData = udpClient.EndReceive(asyncResult, ref remoteEP!);
                    // EndReceive worked and we received data and remote endpoint
                    Console.WriteLine("AsyncResult completed successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving message; broadcasting...\n{e}");
                    Thread.Sleep(1000); // DEBUG leaving this here just in case I get an exception loop again
                    // EndReceive failed
                }
            }
            else
            {
                Console.WriteLine("No message received; broadcasting...");
                // timeout expired
            }
        }

        private static void Send()
        {
            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");
            //udpClient.Client.Bind(udpEndPoint);

            try {
                var data = Encoding.UTF8.GetBytes($"NEW PLAYER:{Battleships.RandomTcpPort}");
                udpClient.Send(data, data.Length, "255.255.255.255", Battleships.BcPort);
            }
            catch (Exception e) {
                Console.WriteLine($"An exception occurred while attempting to broadcast a UDP packet;\n{e}");
            }

            if (firstTry) {
                var msg = $"NEW PLAYER:{Battleships.RandomTcpPort}";
                Console.WriteLine($"Broadcasting message: {msg};");
                Console.WriteLine($"ttl: {udpClient.Ttl}; exclusive address use: {udpClient.ExclusiveAddressUse}; " +
                    $"enable broadcast: {udpClient.EnableBroadcast}; available: {udpClient.Available}");
                firstTry = false;
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            // UdpClient u = ((UdpState)(ar.AsyncState)).u;
            // IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            Debug.Assert(udpClient is not null, "udpClient has not initialised!");
            Debug.Assert(udpEndPoint is not null, "udpEndPoint has not initialised!");

            byte[] receiveBytes = udpClient.EndReceive(ar, ref udpEndPoint);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            if (receiveString != $"NEW PLAYER:{Battleships.RandomTcpPort}"
                && Battleships.PlayerNo == 0) // if player number already assigned, TCP connection already started
            {
                Console.WriteLine($"Received: {receiveString}");
                messageReceived = true;
                contacted = true;
                Battleships.PlayerNo = 2;
                Battleships.AgreedTcpPort = int.Parse(receiveString[11..]); // "NEW PLAYER:".Length = 11; we extract port only
                OpponentConnection.InitiateAsClient();
            }
            else {
                Console.WriteLine("Own message received...");
            }
        }
    }

    internal class UdpState
    {
        // can we replace the UdpState object with our static variables?
            // no, we need it for setting up the asyncResult
        public UdpClient? u;
        public IPEndPoint? e;
        public UdpState(UdpClient? u, IPEndPoint? e)
        {
            this.u = u;
            this.e = e;
        }
    }
}
