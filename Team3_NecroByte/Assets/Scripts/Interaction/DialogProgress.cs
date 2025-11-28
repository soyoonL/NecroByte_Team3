using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogProgress : MonoBehaviour
{
    [SerializeField] public DialogSystem dialogSystem01;
    [SerializeField] public DialogSystem dialogBranch02;
    [SerializeField] public DialogSystem dialogBranch03;

    [SerializeField] public GameObject dialogObject;

    [Header("선택지 UI")]
    public GameObject choiceUI;
    public Button choiceAButton;
    public Button choiceBButton;

    public UnityEvent onDialogFinished;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(StartDialogProgress());
    }

    IEnumerator StartDialogProgress()
    {
        // 기본 대사 진행
        yield return StartCoroutine(RunDialog(dialogSystem01));

        // 선택지 표시
        choiceUI.SetActive(true);

        bool isSelected = false;
        int selected = -1;

        choiceAButton.onClick.RemoveAllListeners();
        choiceBButton.onClick.RemoveAllListeners();

        choiceAButton.onClick.AddListener(() =>
        {
            isSelected = true;
            selected = 0;
        });

        choiceBButton.onClick.AddListener(() =>
        {
            isSelected = true;
            selected = 1;
        });

        // 플레이어가 선택할 때까지 대기
        yield return new WaitUntil(() => isSelected);

        choiceUI.SetActive(false);

        // A 또는 B 루트 시작
        if (selected == 0)
            yield return StartCoroutine(RunDialog(dialogBranch02));
        else
            yield return StartCoroutine(RunDialog(dialogBranch03));

        // 모든 루트가 끝남
        onDialogFinished?.Invoke();

        // 다이얼로그 종료
        yield return new WaitWhile(() => dialogObject.activeSelf);
        gameObject.SetActive(false);
    }

    IEnumerator RunDialog(DialogSystem dialog)
    {
        dialog.gameObject.SetActive(true);

        while (true)
        {
            yield return null;

            // 한 줄 끝났는데 Enter 안 치고 기다릴 수 있게
            if (dialog.IsCurrentLineFinished)
            {
                // Enter 입력 기다림
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    bool isEnd = dialog.UpdateDialog();
                    if (isEnd) break;
                }
            }
            else
            {
                // 아직 타이핑 중이면 UpdateDialog는 Enter로 다음 진행
                dialog.UpdateDialog();
            }
        }
    }
}
