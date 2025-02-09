using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevKit
{
    public class SceneHelper
    {
        public static string GetCurSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public static void ReloadScene()
        {
            Debug.Log(SceneManager.GetActiveScene().buildIndex);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public static int GetActiveScene()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
        public static Vector2 RandomScreenPos(Camera camera)
        {
            var width = Screen.width;
            var height = Screen.height;
            var screenpos=new Vector2(Random.Range(0, width), Random.Range(0, height));
            return camera.ScreenToWorldPoint(screenpos);
        }
    }
}