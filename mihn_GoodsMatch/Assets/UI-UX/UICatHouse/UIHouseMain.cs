using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHouseMain : MonoBehaviour
{
    [SerializeField] UIAnimation _uiAnim;
    [SerializeField] UIListGroup _popupGroupsItem;

    public void OnShowAllCatsInGroup()
    {
        _popupGroupsItem.Show(eHouseDecorType.Cat);
    }

    public void OnShowAllItemsInGroup()
    {
        _popupGroupsItem.Show(eHouseDecorType.Item);
    }

    public void ExitHouse()
    {
        GameStateManager.Idle(null);
    }
}
