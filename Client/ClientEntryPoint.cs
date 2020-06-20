using System;
using NetworkCommon.Data;

namespace Client {
    internal class ClientEntryPoint {
        private static void Main(string[] args) {
            try {
                BrokerClient client = new BrokerClient();
                client.Start(NetworkConnectionConsts.CONNECTION_IP, NetworkConnectionConsts.CONNECTION_PORT);
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            Console.Read();
        }
    }
}
