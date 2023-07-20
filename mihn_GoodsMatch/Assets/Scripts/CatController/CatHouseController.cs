using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatHouseController : CatControl, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(ChangeAnim());
    }
    IEnumerator ChangeAnim()
    {
        anim.AnimationName = "happy";
        anim.Initialize(true);
        yield return new WaitForSeconds(3);
        anim.AnimationName = "idle1";
        anim.Initialize(true);
    }
}
