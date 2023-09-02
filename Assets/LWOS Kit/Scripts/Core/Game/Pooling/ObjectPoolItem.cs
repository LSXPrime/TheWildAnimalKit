using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
	[Serializable]
    public class ObjectPoolItem
    {
        public string Name => Prefab.name;
        public GameObject Prefab;
		public int PrespawnedAmount = 10;

		private Queue<AnimalCore> FreePool = new Queue<AnimalCore>();
		private HashSet<AnimalCore> SpawnedPool = new HashSet<AnimalCore>();
		
		public void Initialize()
		{
			PrespawnItems(PrespawnedAmount);
		}
		
		public AnimalCore Spawn()
		{
			if (FreePool.Count <= 0)
				PrespawnItems(PrespawnedAmount / 2);
			
			if (FreePool.Count <= 0)
				return null;
			
			AnimalCore GO = FreePool.Dequeue();
			SpawnedPool.Add(GO);
			GO.transform.SetParent(null);
			return GO;
		}
		
		public void Despawn(AnimalCore GO)
		{
			GO.gameObject.SetActive(false);
			GO.transform.SetParent(ObjectPool.Instance.PooledParent);
			GO.transform.position = Vector3.zero;
			GO.transform.rotation = Quaternion.identity;

			SpawnedPool.Remove(GO);
			FreePool.Enqueue(GO);
		}
		
		private void PrespawnItems(int count)
		{
			for (int i = 0; i < count; i++)
			{
				AnimalCore GO = GameObject.Instantiate(Prefab).GetComponent<AnimalCore>();
				GO.gameObject.name = Prefab.name;
				GO.transform.SetParent(ObjectPool.Instance.PooledParent);
				GO.gameObject.SetActive(false);
				
				FreePool.Enqueue(GO);
			}
		}
    }
}
