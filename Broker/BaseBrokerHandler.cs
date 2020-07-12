using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class BaseBrokerHandler {
        protected BaseBrokerHandler() { }

        protected static void HandleConnectionException(Exception e, Guid id) {
#if DEBUG
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following exception: {e.Message}\n{e.StackTrace}");
#else
            Console.WriteLine($"Connection with client of ID '{id}' has dropped.");
#endif
        }
        protected static void SendConnectionConfirmation(NetworkStream stream, Guid connectionID) {
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                $"Connection made with broker. Given id '{connectionID}'"
            }));
        }
        protected static void SendBrokerShutdownMessage(NetworkStream stream) {
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                "Broker is shutting down. Connection will be dropped."
            }));
        }
        protected static void SendTopicList(NetworkStream stream, TopicManager topicManager) {
            List<string> names = topicManager.GetTopicNamesList();
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, names.ToArray()));
        }
        protected static void SendMessage(NetworkStream stream, MessagePacket packet) {
            string packetJson = JsonConvert.SerializeObject(packet);
            stream.Write(packetJson.AsASCIIBytes());
        }
    }
}
