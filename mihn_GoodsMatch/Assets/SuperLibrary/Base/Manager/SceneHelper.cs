using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHelper : MonoBehaviour
{
    public static IEnumerator DoLoadSceneAsync(string sceneName)
    {
        yield return instance.LoadScene(sceneName);
    }
    public static void DoLoadScene(string sceneName)
    {
        instance.StartCoroutine(DoLoadSceneAsync(sceneName));
    }


    private static SceneHelper instance;
    public static bool isLoaded { get; private set; }

    private void Awake()
    {
        instance = this;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }
    private void SceneManager_sceneUnloaded(Scene scene)
    {
        Debug.Log("unloaded: " + scene.name);
        isLoaded = false;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        Debug.Log("loaded: " + scene.name);
        isLoaded = false;
        StartCoroutine(WaitForLOad());
    }
    private IEnumerator WaitForLOad()
    {
        yield return null;
        isLoaded = true;
    }

    private IEnumerator LoadScene(string sceneName)
    {
        if (SceneManager.sceneCount > 1)
        {
            var scene = SceneManager.GetSceneAt(1);
            var sceneUnload = SceneManager.UnloadSceneAsync(scene);

            while (!sceneUnload.isDone)
            {
                yield return null;
            }
        }
        yield return null;

        var sceneload = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!sceneload.isDone)
        {
            yield return null;
        }
        if (SceneManager.sceneCount > 1)
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }
}
