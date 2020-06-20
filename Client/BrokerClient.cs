using System;
using System.Net.Sockets;
using System.Threading;
using NetworkCommon.Extensions;

namespace Client {
    public class BrokerClient {
        public void Start(string connectionIP, int port) {
            TcpClient clientTcpClient = new TcpClient(connectionIP, port);
            NetworkStream clientNetworkStream = clientTcpClient.GetStream();

            while (true) {
                Console.WriteLine($"Recieved Message: '{clientNetworkStream.ReadAllDataAsString()}'");
                clientNetworkStream.Write("Return Connection ping".AsASCIIBytes());
                Thread.Sleep(4000);
                clientNetworkStream.Write("Quit".AsASCIIBytes());
                Console.WriteLine("Connection to broker has dropped");
                break;
            }
        }
    }
}
