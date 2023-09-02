using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LWOS
{
	public class MinimapUI : MonoBehaviour
	{
		[Header("Minimap Details")]
		public Sprite MapTexture;
		public Bounds MinimapBounds;
		public float ZoomLevel = 0.6f;
		
		[Header("UI Details")]
		public RectTransform Map;
		public RectTransform PlayerIndicator;
		
		public bool DrawBoundsGizmo;
		public Transform player;
		
		void Start()
		{
			Image map = Map.GetComponent<Image>();
            map.sprite = MapTexture;
            map.preserveAspect = true;
            map.SetNativeSize();

			if (GlobalGameManager.Instance.LocalPlayer != null)
				player = GlobalGameManager.Instance.LocalPlayer.transform;
		}

		void LateUpdate()
		{
			if (!player)
				return;
			
			PlayerIndicator.transform.rotation = Quaternion.Euler (transform.eulerAngles.x, transform.eulerAngles.y, -player.eulerAngles.y);
			Vector2 unitScale = new Vector2 (Map.sizeDelta.x / MinimapBounds.size.x, Map.sizeDelta.y / MinimapBounds.size.z);
			Vector3 MapOffset = MinimapBounds.center - player.position;
			Vector3 MapPos = new Vector3 (MapOffset.x * unitScale.x, MapOffset.z * unitScale.y, 0f) * ZoomLevel;
			Map.localPosition = new Vector2 (MapPos.x, MapPos.y);
			Map.localScale = new Vector3 (ZoomLevel, ZoomLevel, 1f);
		}
		
		void OnDrawGizmos()
		{
			if (!DrawBoundsGizmo)
				return;
			
			Gizmos.color = Color.blue;
			Gizmos.DrawCube(MinimapBounds.center, new Vector3(MinimapBounds.size.x, MinimapBounds.size.y, MinimapBounds.size.z));
		}
		
		private static MinimapUI _Instance;
		public static MinimapUI Instance 
		{
			get 
			{
				if (_Instance == null) 
				{
					_Instance = FindObjectOfType<MinimapUI> ();
				}
				return _Instance;
			}
		}
	}
}