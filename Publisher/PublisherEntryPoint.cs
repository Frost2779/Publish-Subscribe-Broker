using System;

namespace Publisher {
    public class PublisherEntryPoint {
        private static void Main(string[] args) {
            try {
                BrokerPublisher publisher = new BrokerPublisher();
                publisher.Start();
            }
            catch (Exception) { Console.WriteLine("Publisher errored when trying to connect to the broker."); }
            Console.Read();
        }
    }
}
