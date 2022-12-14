using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        DOTween.Kill(txt_Amount);
        DoTextAnim(0, number);
    }

    public void DoTextAnim(int startValue, int endValue, float dur = 1f)
    {
        DOTween.Kill(txt_Amount);
        txt_Amount.text = startValue.ToString();
        txt_Amount.DOText(startValue, endValue, dur);
    }
}
