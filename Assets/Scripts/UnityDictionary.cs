using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

namespace PortalGame {
    /// <summary>
    /// A kind of dictionary that works within the unity editor
    /// and is serializable.
    /// </summary>
    /// <typeparam name="K">The type of the Key</typeparam>
    /// <typeparam name="V">The type of the value</typeparam>
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

        [SerializeField, Tooltip("Defines if this dictionary allows duplicate keys")]
        private bool m_duplicatesAllowed = true;

        /// <summary>
        /// Defines if this dictionary allows duplicate keys.
        /// </summary>
        public bool DuplicatesAllowed {
            get => m_duplicatesAllowed;
            set => m_duplicatesAllowed = value;
        }

        public ICollection<K> Keys => throw new NotImplementedException();

        public ICollection<V> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        [SerializeField]
        private List<KeyValuePair> m_Dictionary = new();

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <remarks>
        /// Ignores <see cref="DuplicatesAllowed"/> property, always overwrites
        /// the first entry with the same key on set. Always gets the first entry.
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not present</exception>
        public V this[K key] {
            get {
                var kvp = m_Dictionary.Find(x => x.Key.Equals(key));
                return kvp == null ? throw new KeyNotFoundException() : kvp.Value;
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

        public void Add(K key, V value) {
            if (!m_duplicatesAllowed && ContainsKey(key)) {
                throw new DuplicateKeyException();
            }

            m_Dictionary.Add(new KeyValuePair { Key = key, Value = value });
        }

        public void Add(KeyValuePair kvp) {
            Add(kvp.Key, kvp.Value);
        }

        public bool ContainsKey(K key) {
            return m_Dictionary.Exists(x => x.Key.Equals(key));
        }

        public bool Remove(K key) {
            return m_Dictionary.RemoveAll(x => x.Key.Equals(key)) > 0;
        }

        public bool Remove(KeyValuePair kvp) {
            return m_Dictionary.RemoveAll(x => x.Key.Equals(kvp.Key) && x.Value.Equals(kvp.Value)) > 0;
        }

        public bool TryGetValue(K key, out V value) {
            var kvp = m_Dictionary.Find(x => x.Key.Equals(key));
            if (kvp == null) {
                value = default;
                return false;
            }

            value = kvp.Value;
            return true;
        }

        public void Clear() {
            m_Dictionary.Clear();
        }

        public bool Contains(KeyValuePair item) {
            return m_Dictionary.Exists(x => x.Key.Equals(item.Key) && x.Value.Equals(item.Value));
        }

        public bool Contains(Predicate<KeyValuePair> match) {
            return m_Dictionary.Exists(match);
        }
    }

    [Serializable]
    public class DuplicateKeyException : Exception {
        public DuplicateKeyException() { }
        public DuplicateKeyException(string message) : base(message) { }
        public DuplicateKeyException(string message, System.Exception inner) : base(message, inner) { }
        protected DuplicateKeyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}