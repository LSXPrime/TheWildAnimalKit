using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace LWOS
{
    public class GameSettings : MonoBehaviour
    {
        [Header("Graphics")]
        public List<GraphicsQuality> Quality = new List<GraphicsQuality>();
        public Text GraphicsLevel;
        public Image GraphicsPreview;
        public Slider QualityLevel;

        [Header("Audio")]
        public Slider MasterSound;
        public Toggle MuteSound;

        [Header("Camera")]
        public Slider CameraSensitivity;
        public Toggle AimAssist;

        void Awake()
        {
            GlobalGameManager.Instance.enabled = false;
            Application.targetFrameRate = 0;
            GetData();
        }

        public void SetGraphicsQuality()
        {
            int level = (int)QualityLevel.value;
            QualitySettings.SetQualityLevel(level);
            GraphicsLevel.text = Quality[level].Name;
            GraphicsPreview.sprite = Quality[level].Preview;
        }

        public void SetFramerate(int framerate)
        {
            Application.targetFrameRate = framerate;
            QualitySettings.vSyncCount = 0;
        }
        public void SetAimAssist()
        {
            GameData.Instance.AimAssist = AimAssist.isOn;
        }

        public void SetAudioLevel()
        {
            AudioListener.volume = MasterSound.value;
        }

        public void MuteAudioSource()
        {
            AudioListener.pause = MuteSound.isOn;
        }

        public void SetPlayerCharacter(int character)
        {
            LocalPrefs.SetInt("PlayerSkin", character);
        }

        public void GetData()
        {
            int GraphicLevel = PlayerPrefs.GetInt("GraphicLevel", 1);
            QualityLevel.value = GraphicLevel;
            QualitySettings.SetQualityLevel(GraphicLevel);
            GraphicsLevel.text = Quality[GraphicLevel].Name;
            GraphicsPreview.sprite = Quality[GraphicLevel].Preview;

            float AudioLevel = PlayerPrefs.GetFloat("AudioLevel", 1f);
            AudioListener.volume = AudioLevel;
            MasterSound.value = AudioLevel;
            bool AudioMute = PlayerPrefs.GetInt("AudioMute", 0) == 0 ? false : true;
            AudioListener.pause = AudioMute;
            MuteSound.isOn = AudioMute;


            float camSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 0.7f);
            CameraSensitivity.value = camSensitivity;
            bool aimAssist = PlayerPrefs.GetInt("AimAssist", 0) == 1;
            AimAssist.isOn = aimAssist;
            SetAimAssist();
        }

        public void SaveData()
        {
            PlayerPrefs.SetInt("GraphicLevel", QualitySettings.GetQualityLevel());
            PlayerPrefs.SetFloat("AudioLevel", AudioListener.volume);
            PlayerPrefs.SetInt("AudioMute", AudioListener.pause ? 1 : 0);
            PlayerPrefs.SetFloat("CameraSensitivity", CameraSensitivity.value);
            PlayerPrefs.SetInt("AimAssist", AimAssist.isOn ? 1 : 0);
        }

        public void Leave()
        {
            Application.Quit();
        }
    }

    [Serializable]
    public class GraphicsQuality
    {
        public string Name;
        public Sprite Preview;
        public UniversalRenderPipelineAsset SRPAsset;
    }
}
