using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PortalGame {
    [Serializable]
    public class UnityDictionary<K, V> : IEnumerable<UnityDictionary<K, V>.KeyValuePair>
                                        where K : Enum {

        [Serializable]
        public class KeyValuePair {
            [field: SerializeField]
            public K Key { get; set; }

            [field: SerializeField]
            public V Value { get; set; }
        }

        [SerializeField]
        private List<KeyValuePair> m_Dictionary = new();

        public V this[K key] {
            get {
                var kvp = m_Dictionary.Find(x => x.Key.Equals(key));
                if (kvp == null) { 
                    throw new KeyNotFoundException();
                }
                return kvp.Value;
            }
            set {
                var kvp = m_Dictionary.Find(x => x.Key.Equals(key));
                if (kvp == null) {
                    m_Dictionary.Add(new KeyValuePair { Key = key, Value = value });
                } else {
                    kvp.Value = value;
                }
            }
        }

        public IEnumerator<KeyValuePair> GetEnumerator() {
            return m_Dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}