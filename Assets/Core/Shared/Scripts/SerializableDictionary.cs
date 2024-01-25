using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FunBoardGames
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {

        [Serializable]
        class Pair
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        List<Pair> items;

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            items = new();

            foreach (var pair in dictionary)
                items.Add(new Pair { key = pair.Key, value = pair.Value });
        }

        public int Count => items.Count;
    
        public TValue this[TKey key] => items.FirstOrDefault(item => item.key.Equals(key)).value;

        public IEnumerable<TKey> Keys => items.Select(item => item.key);

        public IEnumerable<KeyValuePair<TKey, TValue>> GetEnumerator() => items.Select(item => new KeyValuePair<TKey, TValue>(item.key, item.value));
    }
}
