using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITutorial : MonoBehaviour
{
    [SerializeField] Image bgFade;
    
    [SerializeField] GameObject dayTimeTutArrow;
    [SerializeField] GameObject goldTutArrow;
    [SerializeField] GameObject guestMisTutArrow;
    [SerializeField] Button nextTutBtn;
    [SerializeField] float waitToShowNextBtn = 1f;

    [Header("Content panel")]
    [SerializeField] UIAnimation contentAim;
    [SerializeField] Text tutStepText;

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

        DOTween.Kill("Tutorial_FadeBG");
    }
    private void HideAll()
    {
        bgFade.SetAlpha(0f);
        dayTimeTutArrow.SetActive(false);
        goldTutArrow.SetActive(false);
        guestMisTutArrow.SetActive(false);
        tutStepText.text = null;
        nextTutBtn.gameObject.SetActive(false);
    }
    private void OnTutStepDoneHandle(object obj)
    {
        currTutStep = (int)obj + 1;
        switch (currTutStep)
        {
            case 1:
                bgFade.DOFade(0.5f, 0.2f).SetUpdate(true).SetId("Tutorial_FadeBG");
                contentAim.Show(null, () =>
                {
                    ChangeTutText(tutStepText, currTutStep);
                    dayTimeTutArrow.SetActive(true);
                    AddCanvasAndHightlight(dayTimeTutArrow.transform.parent.gameObject);
                    DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                        nextTutBtn.gameObject.SetActive(true);
                    });
                    Time.timeScale = 0;
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
                Time.timeScale = 1f;
                ChangeTutText(tutStepText, currTutStep);
                bgFade.DOFade(0, 0.2f).SetId("Tutorial_FadeBG").SetUpdate(true);
                break;
            case 5:
                ChangeTutText(tutStepText, currTutStep);
                Time.timeScale = 0;
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 6:
                ChangeTutText(tutStepText, currTutStep);
                guestMisTutArrow.SetActive(true);
                DOVirtual.DelayedCall(waitToShowNextBtn, () => {
                    nextTutBtn.gameObject.SetActive(true);
                });
                break;
            case 7:
                guestMisTutArrow.SetActive(false);
                DataManager.UserData.tutBartenderDone = true;
                OnNextTutBtnClicked();
                DataManager.Save();
                Time.timeScale = 1;
                contentAim.Hide(onCompleted: () =>
                {
                    HideAll();
                    gameObject.SetActive(false);   
                });
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
