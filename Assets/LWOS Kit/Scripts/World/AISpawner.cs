using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
	[RequireComponent(typeof(SphereCollider))]
    public class AISpawner : MonoBehaviour
    {
		[Header("Spawn Details")]
		public Transform[] SpawnPoints;
		public int AnimalsID;
		
		[Header("Time Details")]
		public bool SpawnOnStart;
		public bool SpawnOnStartByTrigger;
		public float DurationBetweenSpawns = 1200f;

        [Header("Movement")]
        public Transform[] Waypoints;

        [Header("DEBUG")]
		public bool ShowGizmo;
		public Color GizmoColor = Color.green;
		
		public float timeTmp;
		public bool inRange;
		
        void Start()
        {
			GetComponent<SphereCollider>().isTrigger = true;
			
			if (SpawnOnStart)
				AIManager.Instance.SpawnPack(SpawnPoints, AnimalsID, Waypoints);
			if (SpawnOnStartByTrigger)
				timeTmp = DurationBetweenSpawns;
        }
		
        void Update()
        {
			timeTmp += Time.deltaTime;
			if (timeTmp < DurationBetweenSpawns || !inRange)
				return;
			
			timeTmp = 0f;
			////X Debug.Log("Spawner Triggered");
			AIManager.Instance.SpawnPack(SpawnPoints, AnimalsID, Waypoints);
        }
		
		public void OnTriggerStay(Collider other)
		{
			if (other.CompareTag("Player"))
				inRange = true;
		}
		
		public void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
				inRange = false;
		}
		
		void OnDrawGizmos()
		{
			if (!ShowGizmo || SpawnPoints.Length < 1)
				return;
			
			Gizmos.color = GizmoColor;
			Gizmos.DrawCube(transform.position, new Vector3 (0.5f,0.5f,0.5f));
			for (int i = 0; i < SpawnPoints.Length; i++)
			{
				Gizmos.DrawCube(SpawnPoints[i].position, new Vector3 (0.25f,0.25f,0.25f));
			}
		}
    }
}
