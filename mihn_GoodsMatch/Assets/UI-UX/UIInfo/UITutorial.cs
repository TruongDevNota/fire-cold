using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITutorial : MonoBehaviour
{
    [SerializeField] Image bgFade;
    [SerializeField] Text tutStepText;
    [SerializeField] GameObject dayTimeTutArrow;
    [SerializeField] GameObject goldTutArrow;
    [SerializeField] GameObject guestMisTutArrow;
    [SerializeField] Button nextTutBtn;
    [SerializeField] Text nextTutText;
    [SerializeField] float waitToShowNextBtn = 1f;

    [Header("Order tut")]
    [SerializeField] GameObject orderTurContainer;
    [SerializeField] Text orderTutText;

    [SerializeField] string[] tutContents;

    [SerializeField] int highlightSortingOder = 10;
    private int currTutStep;
    private void Start()
    {
        HideAll();
        nextTutBtn?.onClick.AddListener(OnNextTutBtnClicked);
    }
    private void OnEnable()
    {
        this.RegisterListener((int)EventID.OnTutStepDone, OnTutStepDoneHandle);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener((int)EventID.OnTutStepDone, OnTutStepDoneHandle);
    }
    private void HideAll()
    {
        bgFade.SetAlpha(0f);
        dayTimeTutArrow.SetActive(false);
        goldTutArrow.SetActive(false);
        guestMisTutArrow.SetActive(false);
        tutStepText.text = null;
        nextTutBtn.gameObject.SetActive(false);
        nextTutText.text = "TAP TO CONTINUE";

        orderTurContainer.SetActive(false);
    }
    private void OnTutStepDoneHandle(object obj)
    {
        currTutStep = (int)obj + 1;
        switch (currTutStep)
        {
            case 1:
                bgFade.DOFade(0.8f, 0.2f).OnComplete(() => { ChangeTutText(tutStepText, currTutStep); }).SetUpdate(true);
                Time.timeScale = 0;
                dayTimeTutArrow.SetActive(true);
                AddCanvasAndHightlight(dayTimeTutArrow.transform.parent.gameObject);
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 2:
                RemoveCanvas(dayTimeTutArrow.transform.parent.gameObject);
                dayTimeTutArrow.SetActive(false);
                goldTutArrow.SetActive(true);
                AddCanvasAndHightlight(goldTutArrow.transform.parent.gameObject);
                ChangeTutText(tutStepText, currTutStep);
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 3:
                RemoveCanvas(goldTutArrow.transform.parent.gameObject);
                goldTutArrow.SetActive(false);
                guestMisTutArrow.SetActive(true);
                AddCanvasAndHightlight(guestMisTutArrow.transform.parent.gameObject);
                ChangeTutText(tutStepText, currTutStep);
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 4:
                RemoveCanvas(guestMisTutArrow.transform.parent.gameObject);
                guestMisTutArrow.SetActive(false);
                tutStepText.gameObject.SetActive(false);
                nextTutText.text = "";
                Time.timeScale = 1f;
                bgFade.DOFade(0, 0.2f);
                break;
            case 5:
                orderTurContainer.gameObject.SetActive(true);
                ChangeTutText(orderTutText, currTutStep);
                Time.timeScale = 0;
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 6:
                ChangeTutText(orderTutText, currTutStep);
                guestMisTutArrow.SetActive(true);
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 7:
                DataManager.UserData.tutBartenderDone = true;
                OnNextTutBtnClicked();
                DataManager.Save();
                HideAll();
                Time.timeScale = 1;
                gameObject.SetActive(false);
                break;
        }
    }

    private void AddCanvasAndHightlight(GameObject obj)
    {
        var c = obj.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = highlightSortingOder;
    }
    private void RemoveCanvas(GameObject obj)
    {
        var c = obj.GetComponent<Canvas>();
        if (c != null)
            Destroy(c);
    }
    private void OnNextTutBtnClicked()
    {
        this.PostEvent((int)EventID.OnTutStepDone, currTutStep);
        nextTutBtn.gameObject.SetActive(false);
    }
    private void ChangeTutText(Text textOb, int step)
    {
        var hasContent = currTutStep <= tutContents.Length && !string.IsNullOrEmpty(tutContents[step - 1]);
        textOb.gameObject.SetActive(hasContent);
        textOb.text = hasContent ? tutContents[step - 1] : string.Empty;
    }
}
