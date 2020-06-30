using NetworkCommon.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Broker {
    public class Topic {
        public string Name { get; private set; }
        private readonly List<NetworkStream> _subscribers = new List<NetworkStream>();

        public Topic(string name) {
            Name = name;
        }

        public void AddSubscriber(NetworkStream stream) {
            _subscribers.Add(stream);
        }

        public void RemoveSubscriber(NetworkStream stream) {
            _subscribers.Remove(stream);
        }

        public void ClearSubscribers() {
            _subscribers.Clear();
        }

        public void SendMessage(string message) {
            foreach (NetworkStream stream in _subscribers) {
                stream.Write($"[{Name}] {message}".AsASCIIBytes());
            }
        }
    }

    public class TopicManager {
        private readonly ConcurrentDictionary<Guid, List<Topic>> _topicDictionary = new ConcurrentDictionary<Guid, List<Topic>>();

        public void CreateTopic(Guid pubOwner, string topicName) {
            if (_topicDictionary[pubOwner] == null) {
                _topicDictionary[pubOwner] = new List<Topic>();
            }

            _topicDictionary[pubOwner].Add(new Topic(topicName));
        }

        #region Code Smell
        public List<string> GetTopicNames() {
            List<string> names = new List<string>();

            foreach (List<Topic> topicList in _topicDictionary.Values) {
                foreach (Topic topic in topicList) {
                    names.Add(topic.Name);
                }
            }

            return names;
        }

        public void SubscribeToTopic(string topicName, NetworkStream clientStream) {
            foreach (List<Topic> topicList in _topicDictionary.Values) {
                foreach (Topic topic in topicList) {
                    if (topicName.EqualsIgnoreCase(topic.Name)) {
                        topic.AddSubscriber(clientStream);
                    }
                }
            }
        }

        public void UnsubscribeFromTopic(string topicName, NetworkStream clientStream) {
            foreach (List<Topic> topicList in _topicDictionary.Values) {
                foreach (Topic topic in topicList) {
                    if (topicName.EqualsIgnoreCase(topic.Name)) {
                        topic.RemoveSubscriber(clientStream);
                    }
                }
            }
        }
        #endregion

        public void SendMessage(Guid topicOwner, string topicName, string message) {
            foreach (Topic topic in _topicDictionary[topicOwner]) {
                if (topicName.EqualsIgnoreCase(topic.Name)) {
                    topic.SendMessage(message);
                }
            }
        }
    }
}
