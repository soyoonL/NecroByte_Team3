using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueNPC : MonoBehaviour
{
    public string npcName;

    [Header("Dialog 연결 (씬에 있는 것 연결)")]
    public GameObject dialogSystemObject;   // DialogSystem이 붙은 GameObject
    public DialogSystem dialogSystem;       // 드래그로 연결
    public DialogProgress dialogProgress;   // 드래그로 연결
    public GameObject dialogPage1;          // 네가 사용하는 페이지들
    public GameObject dialogPage2;

    [Header("퀘스트 연결")]
    public string relatedMainQuestID;
    public string relatedSubQuestID;
    public bool isQuestGiver;
    public bool isQuestCompleter;
    public bool requireItemToComplete;

    // 대사 텍스트는 기존 DialogSystem/DialogData로 세팅해서 사용
    // 이 예시는 단순히 이벤트 흐름을 보여주기 위함

    private void Start()
    {
        // 이벤트 구독: 대화가 끝났을 때 OnDialogFinished 로직이 실행되도록 함
        if (dialogProgress != null)
        {
            dialogProgress.onDialogFinished.AddListener(OnDialogFinished);
        }
    }

    // 플레이어가 상호작용(예: InteractableObject.Interact())으로 호출
    public void Interact()
    {
        OpenDialogUI();
    }

    void OpenDialogUI()
    {
        // 네 기존 방식대로 UI 켜기
        dialogSystemObject.SetActive(true);
        dialogSystem.gameObject.SetActive(true);
        dialogProgress.gameObject.SetActive(true);

        // 페이지 활성화(네가 원하면 다이나믹으로 바꿀 수 있음)
        if (dialogPage1) dialogPage1.SetActive(true);
        if (dialogPage2) dialogPage2.SetActive(true);

        // (선택) dialogSystem에 현재 NPC의 dialog 배열을 설정할 수 있음
        // dialogSystem.dialogs = thisNpcDialogArray; // 필요하면 세팅
    }

    // 대화가 끝났을 때 호출되는 콜백
    void OnDialogFinished()
    {
        Debug.Log($"{npcName} dialog finished -> checking quest logic");

        // 메인퀘스트 처리 예시
        if (!string.IsNullOrEmpty(relatedMainQuestID))
        {
            var main = QuestManager.Instance.GetCurrentMainQuest();
            if (main != null && main.questID == relatedMainQuestID)
            {
                if (main.state == QuestState.NotStarted && isQuestGiver)
                {
                    QuestManager.Instance.AcceptMainQuest(main.questID);
                    Debug.Log($"Accepted main quest {main.questID} from {npcName}");
                }
                else if (main.state == QuestState.ReadyToComplete && isQuestCompleter)
                {
                    // (만약 requireItemToComplete이면 이미 ReadyToComplete 상태는 item을 획득했다는 뜻)
                    QuestManager.Instance.CompleteCurrentMainQuest();
                    Debug.Log($"Completed main quest {main.questID} via {npcName}");
                }
            }
        }

        // 서브퀘 처리 예시
        if (!string.IsNullOrEmpty(relatedSubQuestID))
        {
            var sq = QuestManager.Instance.GetSubQuestByID(relatedSubQuestID);
            if (sq != null)
            {
                if (sq.state == QuestState.NotStarted && isQuestGiver)
                {
                    QuestManager.Instance.AcceptSubQuest(sq.questID);
                    Debug.Log($"Accepted sub quest {sq.questID} from {npcName}");
                }
                else if (sq.state == QuestState.ReadyToComplete && isQuestCompleter)
                {
                    QuestManager.Instance.CompleteSubQuest(sq.questID);
                    Debug.Log($"Completed sub quest {sq.questID} via {npcName}");
                }
            }
        }

        // 필요하면 dialog UI 끄기(이미 DialogProgress가 닫는다면 여기서 안 해도 됨)
        // dialogSystemObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (dialogProgress != null)
            dialogProgress.onDialogFinished.RemoveListener(OnDialogFinished);
    }
}
