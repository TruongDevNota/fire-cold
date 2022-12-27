using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStageProcess : MonoBehaviour
{
    [SerializeField] List<Image> _stageImages;
    [SerializeField] Sprite _stageActiveSprite;
    [SerializeField] Sprite _stageDeactiveSprite;

    public void FillStateView(int stageIndex)
    {
        for (int i = 0; i < _stageImages.Count; i++)
            _stageImages[i].sprite = i <= stageIndex ? _stageActiveSprite : _stageDeactiveSprite;
    }
}
