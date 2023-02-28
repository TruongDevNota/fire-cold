using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameContent : MonoBehaviour
{
    [SerializeField] string defaultScene;
    private static LoadGameContent instance { get; set; }

    private string[] tips = new string[]
    {
        "[TIP] Sneaky Sneaky!!!" ,
        "[TIP] Be careful, you're mine!!!",
        "[TIP] Impostor is coming!!!"
    };

    private void Awake()
    {
        instance = this;
    }


    protected string randomTip => tips[UnityEngine.Random.Range(0, tips.Length)];

    public static void PrepairDataToPlay(object data)
    {
        instance.StartCoroutine(instance.DoPrepairDataToPlay(data));
    }

    private IEnumerator DoPrepairDataToPlay(object data)
    {
        //GameStateManager.Init(null);
        //UIToast.ShowLoading(randomTip, 5f, UIToast.IconTip);
        UILoadGame.Init(true, null);

        while (UILoadGame.currentProcess < 0.1f)
        {
            UILoadGame.Process(0, 1, -1, LocalizedManager.Key("base_Loading") + LocalizedManager.Key("base_PleaseWait"));
            yield return null;
        }

        //if (UIToast.Status != UIAnimStatus.IsShow)
        //    UIToast.ShowLoading(randomTip, 5f, UIToast.IconTip);

        MusicManager.Stop(null, false, 0.25f);
        string sceneName = string.IsNullOrEmpty(defaultScene) ? "3_Battle" : defaultScene;

        yield return SceneHelper.DoLoadSceneAsync(sceneName);

        while (!SceneHelper.isLoaded)
            yield return null;
        
        while (GameStateManager.CurrentState == GameState.LoadGame && UILoadGame.currentProcess < 1)
        {
            UILoadGame.Process();
            yield return null;
        }

        GameStateManager.Init(data);
    }

    public void ShowError(FileStatus status)
    {
        string note = "";

        if (status == FileStatus.TimeOut || status == FileStatus.NoInternet)
            note = LocalizedManager.Key("base_DownloadFirstTime") + "\n" + "\n";
        if (status == FileStatus.TimeOut)
        {
            note += LocalizedManager.Key("base_DownloadTimeOut");
        }
        else if (status == FileStatus.NoInternet)
        {
            note += LocalizedManager.Key("base_PleaseCheckYourInternetConnection");
        }
        else
        {
            note += LocalizedManager.Key("base_SomethingWrongs") + "\n ERROR #" + status;
        }
        PopupMes.Show("Oops...!", note, "Ok");
    }
}