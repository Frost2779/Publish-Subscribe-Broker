using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Net.Sockets;

namespace Publisher {
    public class BrokerPublisher : ConnectionClient {

        public void Start() {
            InitBrokerConnection(PacketHandler.I_AM_PUBLISHER_PACKET);
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {

            }
        }

        protected override void IncomingStreamThread(NetworkStream stream) {
            try {
                while (_isClientAlive) {
                    Console.WriteLine(FormatBrokerMessage(stream.ReadAllDataAsString()));
                }
            }
            catch (Exception) { HandleDroppedBrokerConnection(); }
        }

        protected override void PrintInstructions() {

        }
    }
}
