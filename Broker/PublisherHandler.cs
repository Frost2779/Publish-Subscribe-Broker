using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class PublisherHandler : BaseBrokerHandler {
        internal static void InitNewThread(NetworkStream pubNetworkStream, Guid connectionID, TopicManager topicManager, ref bool isBrokerAlive) {
            try {
                SendConnectionConfirmation(pubNetworkStream, connectionID);

                while (isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(pubNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has been closed by the client.");
                        topicManager.RemoveAllTopicsFromPublisher(connectionID);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(pubNetworkStream, topicManager);
                    }
                    else if (packet.PacketType == PacketTypes.CreateTopic) {
                        HandleCreateTopic(pubNetworkStream, connectionID, topicManager, packet);
                    }
                    else if (packet.PacketType == PacketTypes.DeleteTopic) {
                        HandleDeleteTopic(pubNetworkStream, connectionID, topicManager, packet);
                    }
                    else if (packet.PacketType == PacketTypes.TopicMessage) {
                        HandleTopicMessage(pubNetworkStream, connectionID, topicManager, packet);
                    }
                }
                SendBrokerShutdownMessage(pubNetworkStream);
                topicManager.RemoveAllTopicsFromPublisher(connectionID);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private static void HandleCreateTopic(NetworkStream stream, Guid connectionID, TopicManager topicManager, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (topicManager.CreateTopic(connectionID, topicName)) {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has been successfully created."
                }));
                Console.WriteLine($"Publisher '{connectionID}' has created the topic '{topicName}'");
            }
            else {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has failed to be created by the broker. Make sure that the topic doesn't already exist."
                }));
                Console.WriteLine($"Publisher '{connectionID}' has tried to create the topic '{topicName}' but the broker has failed.");
            }
        }
        private static void HandleDeleteTopic(NetworkStream stream, Guid connectionID, TopicManager topicManager, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (topicManager.RemoveTopic(connectionID, topicName)) {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has been successfully deleted."
                }));
                Console.WriteLine($"Publisher '{connectionID}' has deleted the topic '{topicName}'");
            }
            else {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' could not be deleted. Either it doesn't exist or you do not own the topic."
                }));
                Console.WriteLine($"Publisher '{connectionID}' tried to delete the topic '{topicName}'. Either it doesn't exist or they do not own the topic.");
            }
        }
        private static void HandleTopicMessage(NetworkStream stream, Guid connectionID, TopicManager topicManager, MessagePacket packet) {
            string topicName = packet.Data[0];
            string topicMessage = packet.Data[1];

            if (topicManager.SendMessage(connectionID, topicName, topicMessage)) {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have published the message '{topicMessage}' to the topic '{topicName}'"
                }));
                Console.WriteLine($"Publisher '{connectionID}' has sent the message '{topicMessage}' to the topic '{topicName}'");
            }
            else {
                SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Could not send a message to the topic '{topicName}'. Either it doesn't exist or you do not own the topic."
                }));
                Console.WriteLine($"Publisher '{connectionID}' tried to send a message to '{topicName}', but it failed");
            }
        }
    }
}
