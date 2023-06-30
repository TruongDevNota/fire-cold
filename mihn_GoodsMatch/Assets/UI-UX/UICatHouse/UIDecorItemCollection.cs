using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDecorItemCollection : MonoBehaviour
{
    private HouseDataAsset _allgroupData => DataManager.HouseAsset;

    [SerializeField] UIAnimation _uiAnim;
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] RectTransform _container;
    [SerializeField] UIBaseGroupItem _groupPrefab;

    private eHouseDecorType _curListType;
    private List<UIBaseGroupItem> _groups = new List<UIBaseGroupItem>();

    private void Awake()
    {
        _groupPrefab.CreatePool(5);
    }

    private void Start()
    {
        
    }

    public void Show(eHouseDecorType type)
    {
        StartCoroutine(YieldShow(type));
    }

    public void Hide()
    {
        _uiAnim.Hide();
    }

    public IEnumerator YieldShow(eHouseDecorType type)
    {
        _curListType = type;
        yield return YieldFetchData();
        _uiAnim.Show();
    }

    protected IEnumerator YieldFetchData()
    {
        for (int i = 0; i < _allgroupData.allFloorData.Count; i++)
        {
            var existed = i < _groups.Count;
            var group = existed ? _groups[i] : _groupPrefab.Spawn(_container);
            group.Fill(_allgroupData.allFloorData[i], _curListType);
            if(!existed)
                _groups.Add(group);
            yield return new WaitForEndOfFrame();
        }

        for(int j = _groups.Count - 1; j >= _allgroupData.allFloorData.Count; j--)
        {
            _groups[j].Recycle();
            _groups.RemoveAt(j);
            yield return new WaitForEndOfFrame();
        }
    }
}

public enum eHouseDecorType
{
    Item = 1,
    Cat = 2
}
