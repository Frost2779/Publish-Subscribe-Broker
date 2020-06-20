using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Broker {
    public class PubSubBroker {

        private TcpListener _brokerTcpListener = null;

        public void Start() {
            _brokerTcpListener = new TcpListener(IPAddress.Parse(NetworkConnectionConsts.CONNECTION_IP), NetworkConnectionConsts.CONNECTION_PORT);
            _brokerTcpListener.Start();

            while (true) {
                TcpClient brokerClient = _brokerTcpListener.AcceptTcpClient();
                Guid clientConnectionID = Guid.NewGuid();
                Thread handleClientThread = new Thread(() => HandleClientConnection(brokerClient.GetStream(), clientConnectionID));
                handleClientThread.Start();
                Console.WriteLine($"New client connection made with id {clientConnectionID}");
            }
        }

        private void HandleClientConnection(NetworkStream connectionNetworkStream, Guid connectionID) {
            connectionNetworkStream.Write($"Successful server connection. ID given: {connectionID}".AsASCIIBytes());
            while (true) {
                string incomingPacket = connectionNetworkStream.ReadAllDataAsString();
                if(incomingPacket == "Quit") {
                    Console.WriteLine($"Connection {connectionID} has dropped.");
                    break;
                }
            }
        }
    }
}
