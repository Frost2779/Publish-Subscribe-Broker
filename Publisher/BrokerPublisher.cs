using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Text.RegularExpressions;
using static NetworkCommon.Data.MessagePacket;

namespace Publisher {
    public class BrokerPublisher : BrokerConnectionClient {

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
                    Console.Write("What is the topic name you wish to create? Ex: \"This is a topic name\"\n>");
                    HandleCreateCommand(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Delete")) {
                    Console.Write("What is the topic name you wish to delete? Ex: \"This is a topic name to delete\"\n>");
                    HandleDeleteCommand(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Message")) {
                    Console.Write("What is the topic name and message you wish to send? Ex: \"this is the topic name\" \"this is the message to send\"\n>");
                    HandleMessageCommand(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Help")) {
                    PrintInstructions();
                }
                else {
                    Console.WriteLine($"'{userInput}' is an invalid command. Use the 'Help' command to get information about aviable commands and their use.");
                }
            }
        }

        private void HandleCreateCommand(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);

            if (tokens.Count < 1) {
                Console.WriteLine("The 'Create' command requires a string input. Ex: \"this is a topic name\"");
                return;
            }
            string topicName = tokens[TOPIC_NAME].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.CreateTopic, new string[] {
                TrimQuoteMarks(topicName)
            }));
        }

        private void HandleDeleteCommand(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);
            if (tokens.Count < 1) {
                Console.WriteLine("The 'Delete' command requires a string input. Ex: \"this is a topic name\"");
                return;
            }
            string topicName = tokens[TOPIC_NAME].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.DeleteTopic, new string[] {
                TrimQuoteMarks(topicName)
            }));
        }

        private void HandleMessageCommand(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);
            if (tokens.Count < 2) {
                Console.WriteLine("The 'Message' command requires a string input. Ex: \"this is a topic name\" \"this is a topic message\"");
                return;
            }
            string topicName = tokens[TOPIC_NAME].Value;
            string message = tokens[MESSAGE].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.TopicMessage, new string[] {
                TrimQuoteMarks(topicName),
                TrimQuoteMarks(message)
            }));
        }

        protected override void PrintInstructions() {
            Console.WriteLine("Quit - Exits the application and terminates the connection with the broker.\n" +
                              "Help - Prints out this command description block.\n" +
                              "List - Prints out the list of current topics and the GUID of who owns them.\n" +
                              "Create - Begins the process of creating a new topic.\n" +
                              "Delete - Begins the process of deleting a topic.\n" +
                              "Message - Begins the process of sending a message to all subscribers of a topic you own.\n");
        }
    }
}
