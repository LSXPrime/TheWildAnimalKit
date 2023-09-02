using UnityEngine;
using System.Collections;
using MyBox;

public class TakeScreenshot : MonoBehaviour 
{
      public bool controlable = false;
      public float moveSpeed = 1;
      public int resolution = 3; // 1= default, 2= 2x default, etc.
      public string imageName = "Screenshot_";
      public bool resetIndex = false;
	  public KeyCode Take;
      
      private int index = 0;
      
      void Awake()
      {
        if(resetIndex) PlayerPrefs.SetInt("ScreenshotIndex", 0);
        index = PlayerPrefs.GetInt("ScreenshotIndex") != 0 ? PlayerPrefs.GetInt("ScreenshotIndex") : 1;
      }
      
	void Update()
	{
		if(Input.GetKeyDown(Take))
			CaptureScreenshot();
		  
        if(controlable)
		{
            if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
            {
              transform.Translate(0,0,(moveSpeed*Time.deltaTime) * 4);
            }
            else if(Input.GetKey(KeyCode.W))
            {
              transform.Translate(0,0,moveSpeed*Time.deltaTime);
            }
            if(Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
            {
              transform.Translate(0,0,-moveSpeed*Time.deltaTime * 4);
            }
            else if(Input.GetKey(KeyCode.S))
            {
              transform.Translate(0,0,-moveSpeed*Time.deltaTime);
            }
            if(Input.GetKey(KeyCode.A))
            {
              transform.Translate(-moveSpeed*Time.deltaTime, 0, 0);
            }
            else if(Input.GetKey(KeyCode.D))
            {
              transform.Translate(moveSpeed*Time.deltaTime, 0, 0);
            }
		}
	}

	  [ButtonMethod]
      public void CaptureScreenshot()
      {
          ScreenCapture.CaptureScreenshot(imageName + index + ".png", resolution);
          index++;
          ////X Debug.LogWarning("Screenshot saved: " + " --- " + imageName + index);
      }
      
      void OnApplicationQuit()
      {
          PlayerPrefs.SetInt("ScreenshotIndex", (index));
      }
}