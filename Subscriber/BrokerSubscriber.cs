using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using static NetworkCommon.Data.MessagePacket;

namespace Subsciber {
    public class BrokerSubscriber : ConnectionClient {

        public void Start() {
            InitBrokerConnection(new MessagePacket(PacketTypes.InitSubscriberConnection));
            PrintInstructions();
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {
                string userInput = Console.ReadLine();
                if (userInput.EqualsIgnoreCase("Quit")) {
                    ShutdownConnection();
                }
                else if (userInput.EqualsIgnoreCase("List")) {
                    SendNetworkMessage(new MessagePacket(PacketTypes.ListTopics));
                }
                else if (userInput.EqualsIgnoreCase("Sub")) {

                }
                else if (userInput.EqualsIgnoreCase("Unsub")) {

                }
            }
        }

        protected override void PrintInstructions() {

        }
    }
}
