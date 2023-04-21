using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSelectMap : MonoBehaviour
{
    [SerializeField] private UIAnimation anim;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Show()
    {
        anim.Show();
    }

    public void Hide()
    {
        anim.Hide();
    }
    public void SelectMap1()
    {
        DataManager.currnomalMode = nomalMode.Store1;
        GameStateManager.LoadGame(null);
        Hide();
    }
    public void SelectMap2()
    {
        DataManager.currnomalMode = nomalMode.Store2;
        GameStateManager.LoadGame(null);
        Hide();
    }
}
