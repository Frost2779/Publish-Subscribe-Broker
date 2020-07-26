using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    public class PubSubBroker {

        private bool _isBrokerAlive = false;
        private TcpListener _brokerTcpListener = null;

        private readonly TopicManager _topicManager = TopicManager.Instance;
        private ConcurrentDictionary<Guid, BaseBrokerHandler> _publishers = new ConcurrentDictionary<Guid, BaseBrokerHandler>();
        private ConcurrentDictionary<Guid, BaseBrokerHandler> _subscribers = new ConcurrentDictionary<Guid, BaseBrokerHandler>();

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
                        PublisherHandler handler = new PublisherHandler();
                        handler.OnHandlerShutdownEvent += Handler_OnShutdownPublisherEvent;
                        _publishers.TryAdd(clientConnectionID, handler);
                        new Thread(() => handler.InitNewThread(
                                                    connectionStream,
                                                    clientConnectionID
                                                    )).Start();
                        Console.WriteLine($"[Publisher '{clientConnectionID}'] Connection made");
                    }
                    else if (idPacket.PacketType == PacketTypes.InitSubscriberConnection) {
                        SubscriberHandler handler = new SubscriberHandler();
                        handler.OnHandlerShutdownEvent += Handler_OnShutdownSubscriberEvent;
                        _subscribers.TryAdd(clientConnectionID, handler);
                        new Thread(() => handler.InitNewThread(
                                                    connectionStream,
                                                    clientConnectionID
                                                    )).Start();
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

            if (!_isBrokerAlive) {
                foreach (Guid handlerID in _publishers.Keys) {
                    _publishers[handlerID].ShutdownHandler();
                }
                foreach (Guid handlerID in _subscribers.Keys) {
                    _subscribers[handlerID].ShutdownHandler();
                }
            }

        }

        private void Handler_OnShutdownPublisherEvent(Guid handlerID) {
            _publishers.TryRemove(handlerID, out BaseBrokerHandler val);
        }

        private void Handler_OnShutdownSubscriberEvent(Guid handlerID) {
            _subscribers.TryRemove(handlerID, out BaseBrokerHandler val);
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

        #region Util
        private void PrintLocalTopicList() {
            List<string> names = _topicManager.GetTopicNamesList();

            foreach (string topicList in names) {
                Console.WriteLine($"{topicList}");
            }
        }
        private void PrintHelpInstructions() {
            Console.WriteLine("Quit - Exits the application and terminates the connection with the broker.\n" +
                              "Help - Prints out this command description block.\n" +
                              "List - Prints out the list of current topics and the GUID of who owns them.\n");
        }
        #endregion
    }
}
