using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using DG.Tweening;

public class UIPopupListPreview : LazySingleton<UIPopupListPreview>
{
    [SerializeField] UIAnimation _anim;
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] RectTransform _container;
    [SerializeField] Image _itemPrefab;
    private static List<Image> _items = new List<Image>();

    private void Start()
    {
        var samples = _container.GetComponentsInChildren<Image>();
        foreach (var item in samples)
            item.Recycle();
    }

    public static void ShowList(List<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            bool exist = i < _items.Count;
            var current = exist ? _items[i] : Instance._itemPrefab.Spawn(Instance._container);
            current.sprite = sprites[i];
            if (!exist)
                _items.Add(current);
        }
        for (int j = _items.Count - 1; j >= sprites.Count; j--)
        {
            _items[j].Recycle();
            _items.RemoveAt(j);
        }
        Instance._scrollRect.verticalNormalizedPosition = 1f;

        Instance._anim.Show();
    }
}
