using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class SubscriberHandler : BaseBrokerHandler {
        internal static void InitNewThread(NetworkStream subNetworkStream, Guid connectionID, TopicManager topicManager, ref bool isBrokerAlive) {
            try {
                SendConnectionConfirmation(subNetworkStream, connectionID);

                while (isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(subNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has dropped.");
                        topicManager.UnsubscibeFromAll(subNetworkStream);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(subNetworkStream, topicManager);
                    }
                    else if (packet.PacketType == PacketTypes.SubToTopic) {
                        HandleSubToTopic(subNetworkStream, connectionID, topicManager, packet);
                    }
                    else if (packet.PacketType == PacketTypes.UnsubFromTopic) {
                        HandleUnsubFromTopic(subNetworkStream, connectionID, topicManager, packet);
                    }
                }
                SendBrokerShutdownMessage(subNetworkStream);
                topicManager.UnsubscibeFromAll(subNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private static void HandleSubToTopic(NetworkStream stream, Guid connectionID, TopicManager topicManager, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (topicManager.SubscribeToTopic(topicName, stream)) {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have successfully subscribed to the topic '{topicName}'"
                }));
                Console.WriteLine($"Subscriber '{connectionID}' has subscribed to the topic '{topicName}'");
            }
            else {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have failed subscribed to the topic '{topicName}'. This is because the topic doesn't exist."
                }));
                Console.WriteLine($"Subscriber '{connectionID}' tried to subscribed to the topic '{topicName}', but it didn't exist.");
            }
        }
        private static void HandleUnsubFromTopic(NetworkStream stream, Guid connectionID, TopicManager topicManager, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (topicManager.UnsubscribeFromTopic(topicName, stream)) {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have successfully unsubscribed from the topic '{topicName}'"
                }));
                Console.WriteLine($"Subscriber '{connectionID}' has unsubscribed from the topic '{topicName}'");
            }
            else {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have failed to unsubscribed from the topic '{topicName}'. This is because the topic doesn't exist."
                }));
                Console.WriteLine($"Subscriber '{connectionID}' tried to unsubscribed from the topic '{topicName}', but it didn't exist.");
            }
        }
    }
}