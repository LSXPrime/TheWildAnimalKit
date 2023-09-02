using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
    public class ObjectPool : MonoBehaviour
    {
		public Transform PooledParent;
		public List<ObjectPoolItem> Prefabs = new List<ObjectPoolItem>();
		
		public SerializedDictionary<string, ObjectPoolItem> PooledPrefabs = new SerializedDictionary<string, ObjectPoolItem>();

		void Awake()
		{
			foreach (ObjectPoolItem item in Prefabs)
			{
				item.Initialize();
				PooledPrefabs.Add(item.Name, item);
			}
		}
		
		public AnimalCore Spawn(GameObject go, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
		{
			if (!PooledPrefabs.ContainsKey(go.name))
				return null;
			
			AnimalCore GO = PooledPrefabs[go.name].Spawn();
            GO.ResetAI();
            GO.transform.position = position;
			GO.transform.rotation = rotation;
			GO.gameObject.SetActive(true);
			
			return GO;
		}
		
		public void Despawn(AnimalCore GO)
		{
			if (PooledPrefabs.ContainsKey(GO.gameObject.name))
				PooledPrefabs[GO.gameObject.name].Despawn(GO);
		}
		
		public void Despawn(AnimalCore GO, float Time)
		{
			StartCoroutine(TimedDespawn(GO, Time));
		}
		
		IEnumerator TimedDespawn(AnimalCore GO, float Time)
		{
			yield return new WaitForSeconds(Time);
			if (PooledPrefabs.ContainsKey(GO.gameObject.name))
				PooledPrefabs[GO.gameObject.name].Despawn(GO);
		}
		
		private static ObjectPool instance;
		public static ObjectPool Instance
		{
			get
			{
				if (instance == null) { instance = FindObjectOfType<ObjectPool>(); }
				return instance;
			}
		}
    }
}
