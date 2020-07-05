using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using static NetworkCommon.Data.MessagePacket;

namespace Subsciber {
    public class BrokerSubscriber : ConnectionClient {

        public void Start() {
            InitBrokerConnection(new MessagePacket(PacketTypes.InitSubscriberConnection));
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
