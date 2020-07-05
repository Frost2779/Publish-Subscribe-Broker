using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Threading;
using static NetworkCommon.Data.MessagePacket;

namespace NetworkCommon.Connection {
    public abstract class ConnectionClient {
        protected bool _isClientAlive = false;
        private NetworkStream clientNetworkStream;

        public void InitBrokerConnection(MessagePacket connectionPacket) {
            clientNetworkStream = ConnectionHelper.CreateNewConnection();
            string messagePacketJson = JsonConvert.SerializeObject(connectionPacket);

            byte[] packetData = messagePacketJson.AsASCIIBytes();
            clientNetworkStream.Write(packetData, 0, packetData.Length);
            
            
            Thread incomingStreamData = new Thread(() => IncomingStreamThread());
            incomingStreamData.Start();
            _isClientAlive = true;
        }

        protected void IncomingStreamThread() {
            try {
                while (_isClientAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(GetIncomingMessage());
                    if (packet.PacketType == PacketTypes.PrintData) {
                        foreach (string s in packet.Data) {
                            Console.WriteLine(FormatBrokerMessage(s));
                        }
                    }
                    else if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine("Connection has been forcibly closed by the broker. Exiting...");
                        _isClientAlive = false;
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                HandleDroppedBrokerConnection(); 
            }
        }

        protected string FormatBrokerMessage(string s) {
            return $"[Broker] {s}";
        }
        protected void HandleDroppedBrokerConnection() {
            Console.WriteLine("Connection to Broker has dropped");
            _isClientAlive = false;
        }
        protected void SendNetworkMessage(MessagePacket packet) {
            string packetJson = JsonConvert.SerializeObject(packet);
            byte[] messageBytes = packetJson.AsASCIIBytes();
            clientNetworkStream.Write(messageBytes, 0, messageBytes.Length);
        }
        protected string GetIncomingMessage() {
            return clientNetworkStream.ReadAllDataAsString();
        }
        protected abstract void PrintInstructions();
    }
}
