using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Broker {
    public class PubSubBroker {

        private bool _isBrokerAlive = false;
        private TcpListener _brokerTcpListener = null;

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
                        Thread publisherThread = new Thread(() => HandlePublisher(connectionStream, clientConnectionID));
                        publisherThread.Start();
                        Console.WriteLine($"[Publisher '{clientConnectionID}'] Connection made");
                    }
                    else if (idPacket == PacketHandler.I_AM_SUBSCRIBER_PACKET) {
                        Thread publisherThread = new Thread(() => HandleSubscriber(connectionStream, clientConnectionID));
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

            }
        }

        private void HandlePublisher(NetworkStream pubNetworkStream, Guid connectionID) {
            try {
                pubNetworkStream.Write($"Connection made with broker. Given id '{connectionID}'".AsASCIIBytes());

                while (_isBrokerAlive) {
                    string packet = pubNetworkStream.ReadAllDataAsString();

                }
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleSubscriber(NetworkStream subetworkStream, Guid connectionID) {
            try {
                subetworkStream.Write($"Connection made with broker. Given id '{connectionID}'".AsASCIIBytes());

                while (_isBrokerAlive) {
                    string packet = subetworkStream.ReadAllDataAsString();

                }
            }
            catch (Exception e) { HandleConnectionException(e, connectionID); }
        }

        private void HandleConnectionException(Exception e, Guid id) {
            Console.WriteLine($"Connection with client of ID '{id}' has dropped with the following error: \n {e.StackTrace}");
        }
    }
}
