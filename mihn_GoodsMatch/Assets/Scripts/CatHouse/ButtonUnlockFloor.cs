using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonUnlockFloor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] HouseFloor _floor;
    [SerializeField] TextMeshPro _textMesh;

    public void Fill(int price)
    {
        _textMesh.text = price.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("AAAAAAA CLICK ON FLOOR UNLOCK");
    }
}
