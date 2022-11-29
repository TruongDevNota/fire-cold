using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIAnimation))]
public class PopupMes : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    private UIAnimation anim = null;
    public static UIAnimStatus Status { get => instance?.anim != null ? instance.anim.Status : UIAnimStatus.IsHide; }
    public static bool IsAnimation { get => instance?.anim != null ? instance.anim.IsAnimation : false; }

    [Header("Contents")]
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private Text message = null;
    [SerializeField]
    private Button confirmButton = null;
    [SerializeField]
    private Text confirmButtonContent = null;
    [SerializeField]
    private Button cancelButton = null;
    [SerializeField]
    private Text cancelButtonContent = null;

    private static bool CanHideOnHardwareButtonClick = true;

    private static Action actionOnHide = null;

    private static PopupMes instance = null;

    private void Awake()
    {
        instance = this;
    }

    public static void Show(string title, string message, string confirmButton = "Ok", Action onConfirm = null, string cancelButton = "Cancel", Action onCancel = null, bool canHideOnHardwareButton = true)
    {
        if (instance)
        {
            if (IsAnimation)
                return;

            CanHideOnHardwareButtonClick = canHideOnHardwareButton;

            instance.title.text = title;
            instance.message.text = message;

            instance.confirmButtonContent.text = confirmButton;
            instance.confirmButton.onClick.RemoveAllListeners();
            instance.confirmButton.onClick.AddListener(() =>
            {
                actionOnHide = onConfirm;
                Hide();
            });

            if (!string.IsNullOrEmpty(cancelButton))
            {
                instance.cancelButtonContent.text = cancelButton;
                instance.cancelButton.gameObject.SetActive(true);
                instance.cancelButton.onClick.RemoveAllListeners();
                instance.cancelButton.onClick.AddListener(() =>
                {
                    actionOnHide = onCancel;
                    Hide();
                });
            }
            else
            {
                instance.cancelButton.gameObject.SetActive(false);
            }

            instance.anim.Show();
        }
        else
        {
            Debug.LogError("PopupMes NULL");
        }
    }

    public static void Hide()
    {
        instance.anim.Hide(() =>
        {
            if (actionOnHide != null)
            {
                actionOnHide.Invoke();
                actionOnHide = null;
            }
        });
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Status == UIAnimStatus.IsShow && CanHideOnHardwareButtonClick)
                Hide();
        }
    }
}
