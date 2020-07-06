using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Text.RegularExpressions;
using static NetworkCommon.Data.MessagePacket;

namespace Publisher {
    public class BrokerPublisher : ConnectionClient {

        public void Start() {
            InitBrokerConnection(new MessagePacket(PacketTypes.InitPublisherConnection));
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
                else if (userInput.EqualsIgnoreCase("Create")) {
                    if (!HandleCreateCommand(Console.ReadLine())) continue;
                }
                else if (userInput.EqualsIgnoreCase("Delete")) {
                    if (!HandleDeleteCommand(Console.ReadLine())) continue;
                }
                else if (userInput.EqualsIgnoreCase("Message")) {
                    if (!HandleMessageCommand(Console.ReadLine())) continue;
                }
            }
        }

        private bool HandleCreateCommand(string commandInput) {
            string[] tokens = Regex.Split(commandInput, COMMAND_PARSE_REGEX);
            if (tokens.Length < 1) {
                Console.WriteLine("The 'Create' command requires a string input. Ex: Create 'This is a new topic'");
                return false;
            }

            return true;
        }

        private bool HandleDeleteCommand(string commandInput) {
            string[] tokens = Regex.Split(commandInput, COMMAND_PARSE_REGEX);

            return true;
        }

        private bool HandleMessageCommand(string commandInput) {
            string[] tokens = Regex.Split(commandInput, COMMAND_PARSE_REGEX);

            return true;
        }

        protected override void PrintInstructions() {

        }
    }
}
