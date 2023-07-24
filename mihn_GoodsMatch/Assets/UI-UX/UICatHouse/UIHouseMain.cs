using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHouseMain : MonoBehaviour
{
    [SerializeField] UIAnimation _uiAnim;
    [SerializeField] UIDecorItemCollection _popupGroupsItem;
    [SerializeField] UIDecorItemCollection _popupGroupsCat;
    [SerializeField] private Button catBtn;
    [SerializeField] private Button itemBtn;
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnFloorUnlocked, Show);
    }


    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnFloorUnlocked, Show);
    }
    public void Show(object obj)
    {
        _uiAnim.Show();
        catBtn.gameObject.SetActive(DataManager.HouseAsset.allFloorData[0].isUnlocked);
        itemBtn.gameObject.SetActive(DataManager.HouseAsset.allFloorData[0].isUnlocked);
    }
    public void OnShowAllCatsInGroup()
    {
        _popupGroupsCat.Show(eHouseDecorType.Cat);
    }

    public void OnShowAllItemsInGroup()
    {
        _popupGroupsItem.Show(eHouseDecorType.Item);
    }

    public void ExitHouse()
    {
        _uiAnim.Hide();
        GameStateManager.Idle(null);
    }
}
