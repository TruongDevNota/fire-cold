using UnityEngine;
using UnityEngine.UI;

public class UIMainButton : MonoBehaviour
{
    [SerializeField]
    private Image img_Notify;

    private Button btn_Base;
    public Button ButtonBase 
    {
        get
        {
            if(btn_Base == null)
                btn_Base = GetComponent<Button>();
            return btn_Base;
        }
    }

    private void Start()
    {
        btn_Base = GetComponent<Button>();
        btn_Base?.onClick.AddListener(OnButtonClick);
    }

    public System.Action OnButtonClicked;

    public void Fill(bool isActiveNotify = false, System.Action onclick = null)
    {
        img_Notify?.gameObject.SetActive(isActiveNotify);
        OnButtonClicked = onclick;
    }

    public void OnButtonClick()
    {
        OnButtonClicked?.Invoke();
    }
}
