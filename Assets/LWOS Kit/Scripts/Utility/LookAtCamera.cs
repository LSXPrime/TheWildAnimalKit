using UnityEngine;

namespace LBSE
{
	public class LookAtCamera : MonoBehaviour
	{
		public Transform cam;

		void Start()
		{
			cam = Camera.main.transform;
		}

		void Update()
		{
			Vector3 targetPosition = new Vector3 (cam.position.x, transform.position.y, cam.position.z);
			transform.LookAt(targetPosition);
		}
	}
}