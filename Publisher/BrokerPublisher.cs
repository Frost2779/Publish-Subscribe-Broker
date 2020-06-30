using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;

namespace Publisher {
    public class BrokerPublisher : ConnectionClient {

        public void Start() {
            InitBrokerConnection(PacketHandler.I_AM_PUBLISHER_PACKET);
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {
                string userInput = Console.ReadLine();
                if (userInput.EqualsIgnoreCase("Quit")) {
                    _isClientAlive = false;
                }
                else if (userInput.EqualsIgnoreCase("List")) {
                    SendNetworkMessage(PacketHandler.LIST_TOPICS_PACKET);
                }
                else {

                }
            }
        }

        protected override void IncomingStreamThread() {
            try {
                while (_isClientAlive) {
                    Console.WriteLine(FormatBrokerMessage(GetIncomingMessage()));
                }
            }
            catch (Exception) { HandleDroppedBrokerConnection(); }
        }

        protected override void PrintInstructions() {

        }
    }
}
