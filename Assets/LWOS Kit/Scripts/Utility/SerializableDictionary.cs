using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


    [Serializable]
    public class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<K> m_Keys = new List<K>();
        [SerializeField]
        List<V> m_Values = new List<V>();
		
        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();
			
            foreach (var kvp in this)
            {
                m_Keys.Add(kvp.Key);
                m_Values.Add(kvp.Value);
            }
        }
		
        public void OnAfterDeserialize()
        {
            for (int i = 0; i < m_Keys.Count; i++)
                Add(m_Keys[i], m_Values[i]);
			
            m_Keys.Clear();
            m_Values.Clear();
        }
    }