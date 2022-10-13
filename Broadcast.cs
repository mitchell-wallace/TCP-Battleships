using System;
using System.Collections.Generic;
using System.Linq;
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

        private static TimeSpan ttw = TimeSpan.FromSeconds(6);

        public static void Connect()
        {
            udpClient = new UdpClient(Battleships.BcPort);
            // udpClient = new UdpClient();
            udpEndPoint = new IPEndPoint(Battleships.BcAddress, Battleships.BcPort);
            udpState = new UdpState(udpClient, udpEndPoint);

            // manually set some properties because we want to be sure they're set correctly
            // actually I think this breaks stuff.........
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

        public static async void Listen() // we are trying to do BeginReceive and EndReceive here
        {
            if (Battleships.PlayerNo != 0) return;

            Console.WriteLine("Looking for existing game host... <WIP>"); // TODO finish and remove <WIP> text
            var asyncResult = udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

            Console.WriteLine("Listening.....");
            asyncResult.AsyncWaitHandle.WaitOne(ttw);
            if (asyncResult.IsCompleted)
            {
                try
                {
                    IPEndPoint remoteEP = null;
                    byte[] receivedData = udpClient.EndReceive(asyncResult, ref remoteEP);
                    // EndReceive worked and we received data and remote endpoint
                    Console.WriteLine("Message received; listening over!!");
                    Thread.Sleep(1000); // debug use only
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error receiving message; time to broadcast!!");
                    // EndReceive failed
                }
            }
            else
            {
                Console.WriteLine("No message received; time to broadcast!!");
                // timeout expired
            }

        }

        public static void Send()
        {
            //udpClient.Client.Bind(udpEndPoint);

            var msg = $"NEW PLAYER:{Battleships.RandomTcpPort}";
            var data = Encoding.UTF8.GetBytes($"NEW PLAYER:{Battleships.RandomTcpPort}");
            udpClient.Send(data, data.Length, "255.255.255.255", Battleships.BcPort);

            Console.WriteLine($"Broadcasting message: {msg};");
            Console.WriteLine($"ttl: {udpClient.Ttl}; exclusive address use: {udpClient.ExclusiveAddressUse};" +
                $"enable broadcast: {udpClient.EnableBroadcast}; available: {udpClient.Available}");
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
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
        public UdpClient? u;
        public IPEndPoint? e;

        public UdpState(UdpClient? u, IPEndPoint? e)
        {
            this.u = u;
            this.e = e;
        }
    }
}
