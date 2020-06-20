using System.Net.Sockets;
using System.Text;

namespace NetworkCommon.Extensions {
    public static class NetworkStreamUtil {
        /// <summary>
        /// This will read all data currently in the stream and return it as a string. This will lock until there is data to be read.
        /// </summary>
        /// <param name="networkStream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static string ReadAllDataAsString(this NetworkStream networkStream, int bufferSize = 256) {
            byte[] networkReadBuffer = new byte[bufferSize];
            StringBuilder myCompleteMessage = new StringBuilder();
            int numberOfBytesRead;

            do {
                numberOfBytesRead = networkStream.Read(networkReadBuffer, 0, networkReadBuffer.Length);
                myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(networkReadBuffer, 0, numberOfBytesRead));
            }
            while (networkStream.DataAvailable);

            return myCompleteMessage.ToString();
        }

        /// <summary>
        /// Returns a byte array representation of a string using ASCII encoding.
        /// </summary>
        /// <param name="makeBytes"></param>
        /// <returns></returns>
        public static byte[] AsASCIIBytes(this string makeBytes) {
            return Encoding.ASCII.GetBytes(makeBytes);
        }
    }
}
