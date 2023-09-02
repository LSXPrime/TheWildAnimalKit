using UnityEngine;
using UnityEngine.AI;

namespace LWOS
{
    [RequireComponent (typeof(BoxCollider))]
    public class WorldBounds : MonoBehaviour
    {
        void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        Vector3 GetNearstPoint(Vector3 point)
        {
            Vector3 Point = point;
            point.y = 750;

            if (NavMesh.SamplePosition(point, out NavMeshHit nvHit, 1000, 1))
                Point = nvHit.position;
            else
                Point = GlobalGameManager.Instance.RespawnLocations[Random.Range(0, GlobalGameManager.Instance.RespawnLocations.Length - 1)].position;

            return Point;
        }

        private void OnTriggerEnter(Collider other)
        {
            Vector3 TargetPos = GetNearstPoint(other.transform.position);
            other.transform.position = TargetPos;
        }
    }
}
