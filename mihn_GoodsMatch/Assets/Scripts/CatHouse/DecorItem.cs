using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorItem : MonoBehaviour
{
    [SerializeField] SpriteRenderer _SpriteRenderer = null;

    private int _index;
    public int Index
    {
        get { return _index; }
        private set { _index = value; }
    }

    private int _sortingOrderOffset;

    private void Awake()
    {
        if (_SpriteRenderer != null)
            _sortingOrderOffset = _SpriteRenderer.sortingOrder;
    }

    public void SetSortingOrder(int addAmount)
    {
        if (_SpriteRenderer != null)
        {
            _SpriteRenderer.sortingOrder = _sortingOrderOffset + addAmount;
        }
    }
}
