using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Text.RegularExpressions;
using static NetworkCommon.Data.MessagePacket;

namespace Subsciber {
    public class BrokerSubscriber : BrokerConnectionClient {

        public void Start() {
            InitBrokerConnection(new MessagePacket(PacketTypes.InitSubscriberConnection));
            PrintInstructions();
            StartUserInputLoop();
        }

        private void StartUserInputLoop() {
            while (_isClientAlive) {
                string userInput = Console.ReadLine().Trim();
                if (userInput.EqualsIgnoreCase("Quit")) {
                    ShutdownConnection();
                }
                else if (userInput.EqualsIgnoreCase("List")) {
                    SendNetworkMessage(new MessagePacket(PacketTypes.ListTopics));
                }
                else if (userInput.EqualsIgnoreCase("Sub")) {
                    Console.Write("What is the topic name you wish to subscribe to? Ex: \"this is the topic name\"\n>");
                    HandleSubToTopic(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Unsub")) {
                    Console.Write("What is the topic name you wish to unsubscribe from? Ex: \"this is the topic name\"\n>");
                    HandleUnSubFromTopic(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Help")) {
                    PrintInstructions();
                }
                else {
                    Console.WriteLine($"'{userInput}' is an invalid command. Use the 'Help' command to get information about aviable commands and their use.");
                }
            }
        }
        private void HandleSubToTopic(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);
            if (tokens.Count < 1) {
                Console.WriteLine("The 'Sub' command requires a string input. Ex: \"this is a topic name\"");
                return;
            }

            string topicName = tokens[TOPIC_NAME].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.SubToTopic, new string[] {
                TrimQuoteMarks(topicName)
            }));
        }
        private void HandleUnSubFromTopic(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);
            if (tokens.Count < 1) {
                Console.WriteLine("The 'Unsub' command requires a string input. Ex: \"this is a topic name\"");
                return;
            }

            string topicName = tokens[TOPIC_NAME].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.UnsubFromTopic, new string[] {
                TrimQuoteMarks(topicName)
            }));
        }

        protected override void PrintInstructions() {
            Console.WriteLine("Quit - Exits the application and terminates the connection with the broker.\n" +
                              "Help - Prints out this command description block.\n" +
                              "List - Prints out the list of current topics and the GUID of who owns them.\n" +
                              "Sub - Starts the process of subscribing to an existing topic.\n" +
                              "Unsub - Starts the process of unsubscribing from an existing topic.\n");
        }
    }
}
