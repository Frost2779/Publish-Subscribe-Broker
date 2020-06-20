using System;
using System.Security.Cryptography;

namespace Broker {
    class BrokerEntryPoint {
        static void Main(string[] args) {
            PubSubBroker broker = new PubSubBroker();
            broker.Start();

            Console.Read();
        }
    }
}
