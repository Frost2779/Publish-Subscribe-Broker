using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    public class PubSubBroker {

        private bool _isBrokerAlive = false;
        private TcpListener _brokerTcpListener = null;

        private readonly TopicManager _topicManager = new TopicManager();

        #region Init
        public void Start() {
            SpoolTcpListener();
            _isBrokerAlive = true;
            Thread connectionHandler = new Thread(NetworkConnectionHandleThread);
            connectionHandler.Start();
            PrintHelpInstructions();
            Console.WriteLine("Broker started");
            StartUserInputLoop();
        }
        private void SpoolTcpListener() {
            _brokerTcpListener = new TcpListener(IPAddress.Parse(NetworkConsts.CONNECTION_IP), NetworkConsts.CONNECTION_PORT);
            _brokerTcpListener.Start();
        }
        private void NetworkConnectionHandleThread() {
            try {
                while (_isBrokerAlive) {
                    TcpClient brokerClient = _brokerTcpListener.AcceptTcpClient();
                    Guid clientConnectionID = Guid.NewGuid();
                    NetworkStream connectionStream = brokerClient.GetStream();
                    MessagePacket idPacket = JsonConvert.DeserializeObject<MessagePacket>(connectionStream.ReadAllDataAsString());

                    if (idPacket.PacketType == PacketTypes.InitPublisherConnection) {
                        Thread publisherThread = new Thread(() => HandlePublisherThread(connectionStream, clientConnectionID));
                        publisherThread.Start();
                        Console.WriteLine($"[Publisher '{clientConnectionID}'] Connection made");
                    }
                    else if (idPacket.PacketType == PacketTypes.InitSubscriberConnection) {
                        Thread publisherThread = new Thread(() => HandleSubscriberThread(connectionStream, clientConnectionID));
                        publisherThread.Start();
                        Console.WriteLine($"[Subscriber '{clientConnectionID}'] Connection made");
                    }
                    else {
                        Console.WriteLine("Connection made with invalid ID, dropping connection");
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                _isBrokerAlive = false;
            }

        }
        private void StartUserInputLoop() {
            while (_isBrokerAlive) {
                string userInput = Console.ReadLine();

                if (userInput.EqualsIgnoreCase("Quit")) {
                    _isBrokerAlive = false;
                }
                else if (userInput.EqualsIgnoreCase("List")) {
                    PrintLocalTopicList();
                }
                else if (userInput.EqualsIgnoreCase("Help")) {
                    PrintHelpInstructions();
                }
            }
        }
        #endregion

        #region Publisher
        private void HandlePublisherThread(NetworkStream pubNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(pubNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(pubNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has been closed by the client.");
                        _topicManager.RemoveAllTopicsFromPublisher(connectionID);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(pubNetworkStream);
                    }
                    else if (packet.PacketType == PacketTypes.CreateTopic) {
                        HandleCreateTopic(pubNetworkStream, connectionID, packet);
                    }
                    else if (packet.PacketType == PacketTypes.DeleteTopic) {
                        HandleDeleteTopic(pubNetworkStream, connectionID, packet);
                    }
                    else if (packet.PacketType == PacketTypes.TopicMessage) {
                        HandleTopicMessage(pubNetworkStream, connectionID, packet);
                    }
                }
                SendBrokerShutdownMessage(pubNetworkStream);
                _topicManager.RemoveAllTopicsFromPublisher(connectionID);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }
        private void HandleCreateTopic(NetworkStream stream, Guid connectionID, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (_topicManager.CreateTopic(connectionID, topicName)) {
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
        private void HandleDeleteTopic(NetworkStream stream, Guid connectionID, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (_topicManager.RemoveTopic(connectionID, topicName)) {
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
        private void HandleTopicMessage(NetworkStream stream, Guid connectionID, MessagePacket packet) {
            string topicName = packet.Data[0];
            string topicMessage = packet.Data[1];

            if (_topicManager.SendMessage(connectionID, topicName, topicMessage)) {
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
        #endregion

        #region Subscriber
        private void HandleSubscriberThread(NetworkStream subNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(subNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(subNetworkStream.ReadAllDataAsString());

                    if (packet.PacketType == PacketTypes.Disconnect) {
                        Console.WriteLine($"Connection with client of ID '{connectionID}' has dropped.");
                        _topicManager.UnsubscibeFromAll(subNetworkStream);
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(subNetworkStream);
                    }
                    else if (packet.PacketType == PacketTypes.SubToTopic) {
                        HandleSubToTopic(subNetworkStream, connectionID, packet);
                    }
                    else if (packet.PacketType == PacketTypes.UnsubFromTopic) {
                        HandleUnsubFromTopic(subNetworkStream, connectionID, packet);
                    }
                }
                SendBrokerShutdownMessage(subNetworkStream);
                _topicManager.UnsubscibeFromAll(subNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }
        private void HandleSubToTopic(NetworkStream stream, Guid connectionID, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (_topicManager.SubscribeToTopic(topicName, stream)) {
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
        private void HandleUnsubFromTopic(NetworkStream stream, Guid connectionID, MessagePacket packet) {
            string topicName = packet.Data[0];
            if (_topicManager.UnsubscribeFromTopic(topicName, stream)) {
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
        #endregion

        #region Util
        private void PrintLocalTopicList() {
            List<string> names = _topicManager.GetTopicNamesList();

            foreach (string topicList in names) {
                Console.WriteLine($"{topicList}");
            }
        }
        private void HandleConnectionException(Exception e, Guid id) {
#if DEBUG
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following exception: {e.Message}\n{e.StackTrace}");
#else
            Console.WriteLine($"Connection with client of ID '{id}' has dropped.");
#endif
        }
        private void SendConnectionConfirmation(NetworkStream stream, Guid connectionID) {
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                $"Connection made with broker. Given id '{connectionID}'"
            }));
        }
        private void SendBrokerShutdownMessage(NetworkStream stream) {
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, new string[] {
                "Broker is shutting down. Connection will be dropped."
            }));
        }
        private void SendTopicList(NetworkStream stream) {
            List<string> names = _topicManager.GetTopicNamesList();
            SendMessage(stream, new MessagePacket(PacketTypes.PrintData, names.ToArray()));
        }
        private void SendMessage(NetworkStream stream, MessagePacket packet) {
            string packetJson = JsonConvert.SerializeObject(packet);
            stream.Write(packetJson.AsASCIIBytes());
        }
        private void PrintHelpInstructions() {
            Console.WriteLine("Quit - Exits the application and terminates the connection with the broker.\n" +
                              "Help - Prints out this command description block.\n" +
                              "List - Prints out the list of current topics and the GUID of who owns them.\n");
        }
        #endregion
    }
}
