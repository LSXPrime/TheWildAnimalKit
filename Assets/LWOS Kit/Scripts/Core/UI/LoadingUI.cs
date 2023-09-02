using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;



namespace LWOS
{
    public class LoadingUI : MonoBehaviour
    {
        public Image progressImg;
        public float waitTime = 2;
        public UnityEvent onDone;
        private AsyncOperation operation;

        public void LoadScene(int scene)
        {
            gameObject.SetActive(true);
            progressImg.fillAmount = 0;
            StartCoroutine(LoadSceneCO(scene));
        }

        IEnumerator LoadSceneCO(int scene)
        {
            yield return null;

            operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            while (!operation.isDone)
            {
                progressImg.fillAmount = operation.progress / 0.9f;
                Debug.Log("Progress: " + operation.progress);
                yield return new WaitForEndOfFrame();
            }

            if (operation.isDone && progressImg.fillAmount >= 1)
                onDone.Invoke();
        }
    }
}