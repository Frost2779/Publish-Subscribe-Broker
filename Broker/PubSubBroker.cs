using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    public class PubSubBroker {

        private bool _isBrokerAlive = false;
        private TcpListener _brokerTcpListener = null;

        private readonly TopicManager _topicManager = new TopicManager();

        public void Start() {
            SpoolTcpListener();
            _isBrokerAlive = true;
            Thread connectionHandler = new Thread(NetworkConnectionHandleThread);
            connectionHandler.Start();
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
                    PrintTopicList();
                }
                else if (userInput.EqualsIgnoreCase("Help")) {
                    PrintHelpInstructions();
                }
            }
        }
        private void PrintTopicList() {
            foreach (string topicName in _topicManager.GetTopicNames()) {
                Console.WriteLine($"\t[Topic] {topicName}");
            }
        }

        #region Code Smell
        private void HandlePublisherThread(NetworkStream pubNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(pubNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(pubNetworkStream.ReadAllDataAsString());
                    if (packet.PacketType == PacketTypes.Disconnect) {
                        
                        return;
                    }
                    else if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(pubNetworkStream);
                    }
                    else {

                    }
                }
                SendBrokerShutdownMessage(pubNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleSubscriberThread(NetworkStream subNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(subNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    MessagePacket packet = JsonConvert.DeserializeObject<MessagePacket>(subNetworkStream.ReadAllDataAsString());
                    if (packet.PacketType == PacketTypes.ListTopics) {
                        SendTopicList(subNetworkStream);
                    }
                    else {

                    }
                }
                SendBrokerShutdownMessage(subNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }
        #endregion

        private void HandleConnectionException(Exception e, Guid id) {
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following error: \n {e.StackTrace}");
        }
        private void SendConnectionConfirmation(NetworkStream stream, Guid connectionID) {
            stream.Write($"Connection made with broker. Given id '{connectionID}'".AsASCIIBytes());
        }
        private void SendBrokerShutdownMessage(NetworkStream stream) {
            stream.Write("Broker is shutting down. Connection will be dropped.".AsASCIIBytes());
        }
        private void PrintHelpInstructions() {
            Console.WriteLine("<HELP INSTRUCTIONS>");
        }
        private void SendTopicList(NetworkStream stream) {
            StringBuilder builder = new StringBuilder();


        }
    }
}
