using System;

namespace Publisher {
    public class PublisherEntryPoint {
        private static void Main(string[] args) {
            try {
                BrokerPublisher publisher = new BrokerPublisher();
                publisher.Start();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            Console.Read();
        }
    }
}
