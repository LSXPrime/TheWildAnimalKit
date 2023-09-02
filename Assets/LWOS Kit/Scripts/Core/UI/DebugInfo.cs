using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LBSE
{
	//X Class to Trace your FPS on UI
	public class DebugInfo : MonoBehaviour
	{
		public Text Info;
		
		//FPS Counter from the Standard Assets
		const float fpsMeasurePeriod = 0.5f;
		private int FpsAccumulator = 0;
		private float FpsNextPeriod = 0;
		
		void Start()
		{
			if (Info == null)
				Info = GetComponent<Text>();
			
			FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
		}

		void Update()
		{
			FpsAccumulator++;
			if (Time.realtimeSinceStartup > FpsNextPeriod)
			{
				int CurrentFps = (int) (FpsAccumulator/fpsMeasurePeriod);
				FpsAccumulator = 0;
				FpsNextPeriod += fpsMeasurePeriod;
				Info.text = "FPS: " + CurrentFps;
			}
		}
	}
}
