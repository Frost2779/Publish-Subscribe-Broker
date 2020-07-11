using System;

namespace Subsciber {
    public class SubscriberEntryPoint {
        private static void Main(string[] args) {
            try {
                BrokerSubscriber client = new BrokerSubscriber();
                client.Start();
            }
            catch (Exception) { Console.WriteLine("Subscriber errored when trying to connect to the broker."); }
            Console.Read();
        }
    }
}
