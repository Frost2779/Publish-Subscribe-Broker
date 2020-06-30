using NetworkCommon.Extensions;
using System;
using System.Net.Sockets;
using System.Threading;

namespace NetworkCommon.Connection {
    public abstract class ConnectionClient {
        protected bool _isClientAlive = false;
        private NetworkStream clientNetworkStream;

        public void InitBrokerConnection(string connectionIdPacket) {
            clientNetworkStream = ConnectionHelper.CreateNewConnection();
            byte[] packetData = connectionIdPacket.AsASCIIBytes();
            clientNetworkStream.Write(packetData, 0, packetData.Length);
            Thread incomingStreamData = new Thread(() => IncomingStreamThread());
            incomingStreamData.Start();
            _isClientAlive = true;
        }

        protected string FormatBrokerMessage(string s) {
            return $"[Broker] {s}";
        }
        protected void HandleDroppedBrokerConnection() {
            Console.WriteLine("Connection to Broker has dropped");
            _isClientAlive = false;
        }
        protected void SendNetworkMessage(string message) {
            byte[] messageBytes = message.AsASCIIBytes();
            clientNetworkStream.Write(messageBytes, 0, messageBytes.Length);
        }
        protected string GetIncomingMessage() {
            return clientNetworkStream.ReadAllDataAsString();
        }

        protected abstract void IncomingStreamThread();
        protected abstract void PrintInstructions();
    }
}
