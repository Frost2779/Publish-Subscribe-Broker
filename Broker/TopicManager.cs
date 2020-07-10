using NetworkCommon.Data;
using NetworkCommon.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<Guid, List<Topic>> _topicDictionary = new ConcurrentDictionary<Guid, List<Topic>>();
 
        public bool CreateTopic(Guid pubOwner, string topicName) {
            List<Topic> pubTopicList;

            if (!_topicDictionary.TryGetValue(pubOwner, out pubTopicList)) {
                pubTopicList = new List<Topic>();
                _topicDictionary.TryAdd(pubOwner, pubTopicList);
            }

            foreach (Topic t in pubTopicList) {
                if (t.Name.EqualsIgnoreCase(topicName)) {
                    return false;
                }
            }

            pubTopicList.Add(new Topic(topicName));
            return true;
        }

        public bool RemoveTopic(Guid pubOwner, string topicName) {
            List<Topic> pubTopicList;

            if (!_topicDictionary.TryGetValue(pubOwner, out pubTopicList))
                return false;

            for (int i = 0; i < pubTopicList.Count; i++) {
                Topic topic = pubTopicList[i];
                if (topic.Name.EqualsIgnoreCase(topicName)) {
                    topic.SendMessage(new MessagePacket(PacketTypes.PrintData, new string[] {
                        $"The topic named '{topic.Name}' has been deleted and you will no longer recieve messages from it."
                    }));
                    topic.ClearSubscribers();

                    pubTopicList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        #region Code Smell
        public List<string> GetTopicNamesList() {
            List<string> names = new List<string>();

            foreach (Guid key in _topicDictionary.Keys) {
                List<Topic> pubTopicList;
                if (_topicDictionary.TryGetValue(key, out pubTopicList)) {

                    StringBuilder builder = new StringBuilder();

                    for (int i = 0; i < pubTopicList.Count; i++) {
                        string topicName = pubTopicList[i].Name;
                        if (i == 0 && pubTopicList.Count > 1) {
                            builder.Append($"[Owner: {key}] '{topicName}', ");
                        }
                        else if (i == 0) {
                            builder.Append($"[Owner: {key}] '{topicName}'");
                        }
                        else if (i < pubTopicList.Count - 1) {
                            builder.Append($"'{topicName}', ");
                        }
                        else {
                            builder.Append($"'{topicName}'");
                        }
                    }
                    names.Add(builder.ToString());
                }
            }

            return names;
        }
        public void SubscribeToTopic(string topicName, NetworkStream clientStream) {
            foreach (List<Topic> topicList in _topicDictionary.Values) {
                foreach (Topic topic in topicList) {
                    if (topic.Name.EqualsIgnoreCase(topicName)) {
                        topic.AddSubscriber(clientStream);
                    }
                }
            }
        }
        public void UnsubscribeFromTopic(string topicName, NetworkStream clientStream) {
            foreach (List<Topic> topicList in _topicDictionary.Values) {
                foreach (Topic topic in topicList) {
                    if (topic.Name.EqualsIgnoreCase(topicName)) {
                        topic.RemoveSubscriber(clientStream);
                    }
                }
            }
        }
        #endregion

        public bool SendMessage(Guid topicOwner, string topicName, string topicMessage) {
            List<Topic> outPubTopicList;

            if (!_topicDictionary.TryGetValue(topicOwner, out outPubTopicList)) {
                foreach (Topic topic in outPubTopicList) {
                    if (topicName.EqualsIgnoreCase(topic.Name)) {
                        topic.SendMessage(new MessagePacket(PacketTypes.TopicMessage, new string[] {
                            topicName,
                            topicMessage
                        }));

                        return true;
                    }
                }
            }
            return false;
        }
    }
}
