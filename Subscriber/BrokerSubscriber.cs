using NetworkCommon.Connection;
using NetworkCommon.Data;
using System;

namespace Subsciber {
    public class BrokerSubscriber : ConnectionClient {

        public void Start() {
            InitBrokerConnection(PacketHandler.I_AM_SUBSCRIBER_PACKET);
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {

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
