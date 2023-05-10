using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
public class PopupUnlockPack : MonoBehaviour
{
    public Image[] Images;
    public Sprite[] pack1;
    public Sprite[] pack2;
    public Sprite[] pack3;
    public Sprite[] pack4;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ButtonMethod]
    private void Test1()
    {
        for(int i = 0; i < Images.Length; i++)
        {
            Images[i].sprite = pack1[i];
        }
    }
    [ButtonMethod]
    private void Test2()
    {
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i].sprite = pack2[i];
        }
    }
    [ButtonMethod]
    private void Test3()
    {
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i].sprite = pack3[i];
        }
    }
    [ButtonMethod]
    private void Test4()
    {
        for (int i = 0; i < Images.Length; i++)
        {
            Images[i].sprite = pack4[i];
        }
    }
}
