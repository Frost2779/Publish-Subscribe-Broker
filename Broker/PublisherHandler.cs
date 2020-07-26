using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    internal class PublisherHandler : BaseBrokerHandler {
        internal void InitNewThread(NetworkStream pubNetworkStream, Guid connectionID) {
            HandlerNetworkStream = pubNetworkStream;
            ConnectionID = connectionID;

            try {
                SendConnectionConfirmation();

                while (IsBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(pubNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has been closed by the client.");
                        TopicManagerInstance.RemoveAllTopicsFromPublisher(connectionID);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList();
                    }
                    else if (packet.PacketType == PacketTypes.CreateTopic) {
                        HandleCreateTopic(packet);
                    }
                    else if (packet.PacketType == PacketTypes.DeleteTopic) {
                        HandleDeleteTopic(packet);
                    }
                    else if (packet.PacketType == PacketTypes.TopicMessage) {
                        HandleTopicMessage(packet);
                    }
                }
                SendBrokerShutdownMessage();
                TopicManagerInstance.RemoveAllTopicsFromPublisher(connectionID);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleCreateTopic(MessagePacket packet) {
            string topicName = packet.Data[0];
            if (TopicManagerInstance.CreateTopic(ConnectionID, topicName)) {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has been successfully created."
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' has created the topic '{topicName}'");
            }
            else {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has failed to be created by the broker. Make sure that the topic doesn't already exist."
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' has tried to create the topic '{topicName}' but the broker has failed.");
            }
        }
        private void HandleDeleteTopic(MessagePacket packet) {
            string topicName = packet.Data[0];
            if (TopicManagerInstance.RemoveTopic(ConnectionID, topicName)) {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' has been successfully deleted."
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' has deleted the topic '{topicName}'");
            }
            else {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Topic of name '{topicName}' could not be deleted. Either it doesn't exist or you do not own the topic."
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' tried to delete the topic '{topicName}'. Either it doesn't exist or they do not own the topic.");
            }
        }
        private void HandleTopicMessage(MessagePacket packet) {
            string topicName = packet.Data[0];
            string topicMessage = packet.Data[1];

            if (TopicManagerInstance.SendMessage(ConnectionID, topicName, topicMessage)) {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"You have published the message '{topicMessage}' to the topic '{topicName}'"
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' has sent the message '{topicMessage}' to the topic '{topicName}'");
            }
            else {
                SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                    $"Could not send a message to the topic '{topicName}'. Either it doesn't exist or you do not own the topic."
                }));
                Console.WriteLine($"Publisher '{ConnectionID}' tried to send a message to '{topicName}', but it failed");
            }
        }
    }
}
