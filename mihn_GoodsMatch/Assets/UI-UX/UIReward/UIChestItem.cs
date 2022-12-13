using UnityEngine;
using UnityEngine.UI;

public class UIChestItem : MonoBehaviour
{
    [SerializeField]
    Image img_icon;
    [SerializeField]
    Text txt_Amount;

    public void Fill(int number, Sprite icon = null)
    {
        if(icon != null)
            img_icon.sprite = icon;

        txt_Amount.text = $"+{number}";
    }
}
