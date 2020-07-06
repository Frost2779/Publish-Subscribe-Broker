﻿using NetworkCommon.Connection;
using NetworkCommon.Data;
using NetworkCommon.Extensions;
using System;
using System.Text.RegularExpressions;
using static NetworkCommon.Data.MessagePacket;

namespace Publisher {
    public class BrokerPublisher : ConnectionClient {

        private const int TOPIC_NAME = 0;

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
                    Console.WriteLine("What is the topic name you wish to create?");
                    HandleCreateCommand(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Delete")) {
                    Console.WriteLine("What is the topic name you wish to delete?");
                    HandleDeleteCommand(Console.ReadLine());
                }
                else if (userInput.EqualsIgnoreCase("Message")) {
                    Console.WriteLine("What is the topic name and message you wish to send?");
                    HandleMessageCommand(Console.ReadLine());
                }
            }
        }

        private void HandleCreateCommand(string commandInput) {
            MatchCollection tokens = Regex.Matches(commandInput, COMMAND_PARSE_REGEX);

            if (tokens.Count < 1) {
                Console.WriteLine("The 'Create' command requires a string input. Ex: 'this is a topic name'");
                return;
            }
            string topicName = tokens[TOPIC_NAME].Value;

            SendNetworkMessage(new MessagePacket(PacketTypes.CreateTopic, new string[] {
                TrimQuoteMarks(topicName)
            })); ;
        }

        private void HandleDeleteCommand(string commandInput) {

        }

        private void HandleMessageCommand(string commandInput) {

        }

        protected override void PrintInstructions() {

        }
    }
}
