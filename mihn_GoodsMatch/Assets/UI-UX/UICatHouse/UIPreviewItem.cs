using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPreviewItem : MonoBehaviour
{
    private HouseDataAsset _allItemData => DataManager.HouseAsset;

    [SerializeField] UIAnimation _uiAnim;
    [SerializeField] Image _imgItem;
    [SerializeField] Text _txtItemName;

    private void Start()
    {
        this.RegisterListener((int)EventID.ShowItemPreview, OnShow);
    }

    public void OnShow(object obj)
    {
        var datum = (ItemPreivewDatum)obj;
        var itemDatum = _allItemData.GetItemData(datum.floorIndex, datum.itemID, datum.type);

        if(itemDatum != null)
        {
            _uiAnim.Show(onStart: () =>
            {
                _imgItem.sprite = itemDatum.thumbUnlocked;
                _txtItemName.text = itemDatum.name;
            });
        }
        else
        {
            Debug.LogError($"Itemdatum not found: floor-{datum.floorIndex}, itemId -{datum.itemID}");
        }
    }
}
