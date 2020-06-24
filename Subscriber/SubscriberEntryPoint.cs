using System;

namespace Subsciber {
    public class SubscriberEntryPoint {
        private static void Main(string[] args) {
            try {
                BrokerSubscriber client = new BrokerSubscriber();
                client.Start();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            Console.Read();
        }
    }
}
