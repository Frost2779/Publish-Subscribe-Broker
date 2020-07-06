using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Threading;
using static NetworkCommon.Data.MessagePacket;

namespace NetworkCommon.Connection {
    public abstract class ConnectionClient {
        protected const string COMMAND_PARSE_REGEX = @"('{1}[A-z| ]+'{1})";
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
        protected void ShutdownConnection() {
            SendNetworkMessage(new MessagePacket(PacketTypes.Disconnect));
            Console.WriteLine("Connection to the broker has been closed by the client.");
            _isClientAlive = false;
        }
        protected string GetIncomingMessage() {
            return clientNetworkStream.ReadAllDataAsString();
        }
        protected string TrimQuoteMarks(string s) {
            string trimmed = s.Substring(1, s.Length - 2);

            return trimmed;
        }
        protected abstract void PrintInstructions();
    }
}
