using NetworkCommon.Data;
using System.Net.Sockets;

namespace NetworkCommon.Connection {
    public static class ConnectionHelper {
        public static NetworkStream CreateNewConnection(
            string connectionIP = NetworkConsts.CONNECTION_IP,
            int connectionPort = NetworkConsts.CONNECTION_PORT) {

            TcpClient client = new TcpClient(connectionIP, connectionPort);
            return client.GetStream();
        }
    }
}
