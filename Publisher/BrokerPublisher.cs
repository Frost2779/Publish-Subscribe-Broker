using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using static NetworkCommon.Data.MessagePacket;

namespace Publisher {
    public class BrokerPublisher : ConnectionClient {

        public void Start() {
            InitBrokerConnection(new MessagePacket(PacketTypes.InitPublisherConnection));
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {
                string userInput = Console.ReadLine();
                if (userInput.EqualsIgnoreCase("Quit")) {
                    _isClientAlive = false;
                }
                else if (userInput.EqualsIgnoreCase("List")) {
                    SendNetworkMessage(new MessagePacket(PacketTypes.ListTopics));
                }
                else if (userInput.EqualsIgnoreCase("Create")) {

                }
                else if (userInput.EqualsIgnoreCase("Delete")) {

                }
                else if (userInput.EqualsIgnoreCase("Message")) {

                }
            }
        }

        protected override void PrintInstructions() {

        }
    }
}
