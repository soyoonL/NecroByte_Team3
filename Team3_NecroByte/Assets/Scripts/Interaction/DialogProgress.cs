using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static DialogSystem;

public class DialogProgress : MonoBehaviour
{
    [SerializeField]
    public DialogSystem dialogSystem01;

    [SerializeField]
    public GameObject dialogObject;

    public UnityEvent onDialogFinished;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(StartDialogProgress());
    }

    public IEnumerator StartDialogProgress()
    {
        // 첫 번째 대사 분기 시작
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());

        // 대사가 끝난 시점(=여기)에서 콜백 호출
        onDialogFinished?.Invoke();

        // 대사 분기 사이 원하는 행동 추가 가능
        // 캐릭터를 움직이거나 아이템을 획득하는 등의

        yield return new WaitWhile(() => dialogObject.activeSelf);
        gameObject.SetActive(false);
    }
}
