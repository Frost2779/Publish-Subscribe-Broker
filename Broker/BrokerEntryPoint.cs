#define DEBUG

using System;

namespace Broker {
    public class BrokerEntryPoint {

        private static void Main(string[] args) {
#if DEBUG
            Console.WriteLine("WARNING: DEBUG MODE IS ENABLED, DISABLE BEFORE RELEASE. FOUND IN Broker/BrokerEntryPoint.cs");
#endif
            PubSubBroker broker = new PubSubBroker();
            broker.Start();

            Console.Read();
        }
    }
}
