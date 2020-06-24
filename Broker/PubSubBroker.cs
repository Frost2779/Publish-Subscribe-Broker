﻿using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Broker {
    public class PubSubBroker {

        private bool _isBrokerAlive = false;
        private TcpListener _brokerTcpListener = null;

		private ConcurrentDictionary<string, string> _topicDictionary; //Types are placeholder at the moment

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
                    string idPacket = connectionStream.ReadAllDataAsString();

                    if (idPacket == PacketHandler.I_AM_PUBLISHER_PACKET) {
                        Thread publisherThread = new Thread(() => HandlePublisherThread(connectionStream, clientConnectionID));
                        publisherThread.Start();
                        Console.WriteLine($"[Publisher '{clientConnectionID}'] Connection made");
                    }
                    else if (idPacket == PacketHandler.I_AM_SUBSCRIBER_PACKET) {
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
                    BeginBrokerShutdown();
                }
                else if (userInput.EqualsIgnoreCase("List Topics")) {

                }
                else if (userInput.EqualsIgnoreCase("List Connections")) { 
                
                }
            }
        }

        private void BeginBrokerShutdown() {
            _isBrokerAlive = false;
        }

        private void HandlePublisherThread(NetworkStream pubNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(pubNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    string packet = pubNetworkStream.ReadAllDataAsString();

                }
                SendBrokerShutdownMessage(pubNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleSubscriberThread(NetworkStream subNetworkStream, Guid connectionID) {
            try {
                SendConnectionConfirmation(subNetworkStream, connectionID);

                while (_isBrokerAlive) {
                    string packet = subNetworkStream.ReadAllDataAsString();

                }
                SendBrokerShutdownMessage(subNetworkStream);
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleConnectionException(Exception e, Guid id) {
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following error: \n {e.StackTrace}");
        }
        private void SendConnectionConfirmation(NetworkStream stream, Guid connectionID) {
            stream.Write($"Connection made with broker. Given id '{connectionID}'".AsASCIIBytes());
        }
        private void SendBrokerShutdownMessage(NetworkStream stream) {
            stream.Write("Broker is shutting down. Connection will be dropped.".AsASCIIBytes());
        }

    }
}
