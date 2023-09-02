using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerSpawner : MonoBehaviour
    {
        public int ID;

        void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                GlobalGameManager.Instance.LocalPlayerController.Status.SpawnPointID = ID;
        }
    }
}
