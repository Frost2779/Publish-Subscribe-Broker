using Newtonsoft.Json;

namespace NetworkCommon.Data {
    public class MessagePacket {
        public enum PacketTypes : byte { InitPublisherConnection, InitSubscriberConnection, Disconnect, ListTopics, CreateTopic, DeleteTopic, MessageTopic, SubToTopic, UnsubFromTopic, PrintData }

        [JsonProperty]
        public PacketTypes PacketType;

        [JsonProperty]
        public string[] Data;

        public MessagePacket(PacketTypes type) : this(type, new string[0]) { }

        [JsonConstructor]
        public MessagePacket(PacketTypes type, string[] data) {
            PacketType = type;
            Data = data;
        }
    }
}
