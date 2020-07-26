using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class BaseBrokerHandler {
        public delegate void OnHandlerShutdown(Guid handlerID);
        public event OnHandlerShutdown OnHandlerShutdownEvent;

        protected NetworkStream HandlerNetworkStream;
        protected Guid ConnectionID;
        protected TopicManager TopicManagerInstance = TopicManager.Instance;
        protected bool IsBrokerAlive = true;

        protected BaseBrokerHandler() { }

        public void ShutdownHandler() {
            IsBrokerAlive = false;
            OnHandlerShutdownEvent.Invoke(ConnectionID);
        }

        protected void HandleConnectionException(Exception e, Guid id) {
#if DEBUG
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following exception: {e.Message}\n{e.StackTrace}");
#else
            Console.WriteLine($"Connection with client of ID '{id}' has dropped.");
#endif
        }
        protected void SendConnectionConfirmation() {
            SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                $"Connection made with broker. Given id '{ConnectionID}'"
            }));
        }
        protected void SendBrokerShutdownMessage() {
            SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                "Broker is shutting down. Connection will be dropped."
            }));
        }
        protected void SendTopicList() {
            List<string> names = TopicManagerInstance.GetTopicNamesList();
            SendMessage(new MessagePacket(PacketTypes.PrintData, names.ToArray()));
        }
        protected void SendMessage(MessagePacket packet) {
            string packetJson = JsonConvert.SerializeObject(packet);
            HandlerNetworkStream.Write(packetJson.AsASCIIBytes());
        }
    }
}
