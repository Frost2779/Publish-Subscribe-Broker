using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class SubscriberHandler : BaseBrokerHandler {
        internal void InitNewThread(NetworkStream subNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation();

                while (IsBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(subNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has dropped.");
                        TopicManagerInstance.UnsubscibeFromAll(subNetworkStream);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList();
                    }
                    else if (packet.PacketType == PacketTypes.SubToTopic) {
                        HandleSubToTopic(packet);
                    }
                    else if (packet.PacketType == PacketTypes.UnsubFromTopic) {
                        HandleUnsubFromTopic(packet);
                    }
                }
                SendBrokerShutdownMessage();
                TopicManagerInstance.UnsubscibeFromAll(subNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleSubToTopic(MessagePacket packet) {
            string topicName = packet.Data[0];
            if (TopicManagerInstance.SubscribeToTopic(topicName, HandlerNetworkStream)) {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have successfully subscribed to the topic '{topicName}'"
                }));
                Console.WriteLine($"Subscriber '{ConnectionID}' has subscribed to the topic '{topicName}'");
            }
            else {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have failed subscribed to the topic '{topicName}'. This is because the topic doesn't exist."
                }));
                Console.WriteLine($"Subscriber '{ConnectionID}' tried to subscribed to the topic '{topicName}', but it didn't exist.");
            }
        }
        private void HandleUnsubFromTopic(MessagePacket packet) {
            string topicName = packet.Data[0];
            if (TopicManagerInstance.UnsubscribeFromTopic(topicName, HandlerNetworkStream)) {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have successfully unsubscribed from the topic '{topicName}'"
                }));
                Console.WriteLine($"Subscriber '{ConnectionID}' has unsubscribed from the topic '{topicName}'");
            }
            else {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have failed to unsubscribed from the topic '{topicName}'. This is because the topic doesn't exist."
                }));
                Console.WriteLine($"Subscriber '{ConnectionID}' tried to unsubscribed from the topic '{topicName}', but it didn't exist.");
            }
        }
    }
}