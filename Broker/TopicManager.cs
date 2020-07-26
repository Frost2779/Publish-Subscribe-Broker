using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using static NetworkCommon.Data.MessagePacket;

namespace Broker {
    public class Topic {
        public string Name { get; private set; }
        private readonly List<NetworkStream> _subscribers = new List<NetworkStream>();

        public Topic(string name) {
            Name = name;
        }

        public void AddSubscriber(NetworkStream stream) {
            lock (_subscribers) {
                _subscribers.Add(stream);
            }
        }

        public void RemoveSubscriber(NetworkStream stream) {
            lock (_subscribers) {
                _subscribers.Remove(stream);
            }
        }

        public void ClearSubscribers() {
            lock (_subscribers) {
                _subscribers.Clear();
            }
        }

        public void SendMessage(MessagePacket packet) {
            lock (_subscribers) {
                string packetJson = JsonConvert.SerializeObject(packet);

                foreach (NetworkStream stream in _subscribers) {
                    stream.Write(packetJson.AsASCIIBytes());
                }
            }
        }
    }

    public class TopicManager {
        private readonly ConcurrentDictionary<Guid, Dictionary<string, Topic>> _topicDictionary = new ConcurrentDictionary<Guid, Dictionary<string, Topic>>();

        private TopicManager() { }

        public static TopicManager Instance {
            get {
                lock (_instance) {
                    if (_instance == null)
                        _instance = new TopicManager();
                    return _instance;
                }
            }
        }
        private static TopicManager _instance = null;

        public bool CreateTopic(Guid pubOwner, string topicName) {

            if (!_topicDictionary.TryGetValue(pubOwner, out Dictionary<string, Topic> pubTopicDictionary)) {
                pubTopicDictionary = new Dictionary<string, Topic>();
                _topicDictionary.TryAdd(pubOwner, pubTopicDictionary);
            }

            if (pubTopicDictionary.ContainsKey(topicName))
                return false;

            pubTopicDictionary.Add(topicName, new Topic(topicName));
            return true;
        }
        public bool RemoveTopic(Guid pubOwner, string topicName) {

            if (!_topicDictionary.TryGetValue(pubOwner, out Dictionary<string, Topic> pubTopicDictionary))
                return false;

            if (pubTopicDictionary.ContainsKey(topicName)) {
                Topic topic = pubTopicDictionary[topicName];
                topic.SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                        $"The topic named '{topic.Name}' has been deleted and you will no longer recieve messages from it."
                    }));
                topic.ClearSubscribers();

                pubTopicDictionary.Remove(topicName);
                return true;
            }
            return false;
        }
        public void RemoveAllTopicsFromPublisher(Guid pubOwner) {
            _topicDictionary.TryRemove(pubOwner, out Dictionary<string, Topic> outTopicList);
        }
        public void UnsubscibeFromAll(NetworkStream stream) {
            foreach (Dictionary<string, Topic> topicDictionary in _topicDictionary.Values) {
                foreach (Topic topic in topicDictionary.Values) {
                    topic.RemoveSubscriber(stream);
                }
            }
        }
        public List<string> GetTopicNamesList() {
            List<string> names = new List<string>();

            foreach (Guid key in _topicDictionary.Keys) {
                _topicDictionary.TryGetValue(key, out Dictionary<string, Topic> pubTopicDictionary);
                StringBuilder builder = new StringBuilder();

                List<string> topicNames = pubTopicDictionary.Keys.ToList();

                for (int i = 0; i < topicNames.Count; i++) {
                    string topicName = topicNames[i];
                    if (i == 0 && topicNames.Count > 1) {
                        builder.Append($"[Owner: {key}] '{topicName}', ");
                    }
                    else if (i == 0) {
                        builder.Append($"[Owner: {key}] '{topicName}'");
                    }
                    else if (i < topicNames.Count - 1) {
                        builder.Append($"'{topicName}', ");
                    }
                    else {
                        builder.Append($"'{topicName}'");
                    }
                }
                names.Add(builder.ToString());
            }

            return names;
        }
        public bool SubscribeToTopic(string topicName, NetworkStream clientStream) {
            Topic subTopic;
            if ((subTopic = FindTopic(topicName)) != null) {
                subTopic.AddSubscriber(clientStream);
                return true;
            }
            return false;
        }
        public bool UnsubscribeFromTopic(string topicName, NetworkStream clientStream) {
            Topic unSubTopic;
            if ((unSubTopic = FindTopic(topicName)) != null) {
                unSubTopic.RemoveSubscriber(clientStream);
                return true;
            }
            return false;
        }
        public bool SendMessage(Guid topicOwner, string topicName, string topicMessage) {
            if (_topicDictionary.TryGetValue(topicOwner, out Dictionary<string, Topic> outPubTopicList)) {
                if (outPubTopicList.ContainsKey(topicName)) {
                    outPubTopicList[topicName].SendMessage(new MessagePacket(PacketTypes.TopicMessage, new string[] {
                                topicName,
                                topicMessage
                            }));
                }
            }
            return false;
        }
        private Topic FindTopic(string topicName) {
            Topic found = null;
            foreach (Dictionary<string, Topic> topicDictionary in _topicDictionary.Values) {
                if (topicDictionary.ContainsKey(topicName)) {
                    return topicDictionary[topicName];
                }
            }
            return found;
        }
    }
}