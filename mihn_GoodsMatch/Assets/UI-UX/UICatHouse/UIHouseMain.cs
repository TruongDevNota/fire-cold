using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHouseMain : MonoBehaviour
{
    [SerializeField] UIAnimation _uiAnim;
    [SerializeField] UIDecorItemCollection _popupGroupsItem;
    [SerializeField] UIDecorItemCollection _popupGroupsCat;

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
