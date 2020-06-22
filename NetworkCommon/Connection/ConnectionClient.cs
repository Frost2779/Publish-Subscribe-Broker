using NetworkCommon.Extensions;
using System;
using System.Net.Sockets;
using System.Threading;

namespace NetworkCommon.Connection {
    public abstract class ConnectionClient {
        protected bool _isClientAlive = false;

        public void InitBrokerConnection(string connectionIdPacket) {
            NetworkStream clientNetworkStream = ConnectionHelper.CreateNewConnection();
            byte[] packetData = connectionIdPacket.AsASCIIBytes();
            clientNetworkStream.Write(packetData, 0, packetData.Length);
            Thread incomingStreamData = new Thread(() => IncomingStreamThread(clientNetworkStream));
            incomingStreamData.Start();
            _isClientAlive = true;
        }

        protected string FormatBrokerMessage(string s) { return $"[Broker] {s}"; }
        protected void HandleDroppedBrokerConnection() {
            Console.WriteLine("Connection to Broker has dropped");
            _isClientAlive = false;
        }

        protected abstract void IncomingStreamThread(NetworkStream stream);
        protected abstract void PrintInstructions();
    }
}
