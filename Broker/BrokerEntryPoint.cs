using System;

namespace Broker {
    public class BrokerEntryPoint {
        private static void Main(string[] args) {
            PubSubBroker broker = new PubSubBroker();
            broker.Start();

            Console.Read();
        }
    }
}
