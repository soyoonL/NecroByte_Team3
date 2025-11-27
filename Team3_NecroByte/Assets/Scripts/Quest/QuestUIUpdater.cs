using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestUIUpdater : MonoBehaviour
{
    [Header("메인 퀘스트 UI")]
    public TextMeshProUGUI mainQuestNameText;

    [Header("서브 퀘스트 UI")]
    public TextMeshProUGUI subQuestNameText; // 서브 퀘스트를 표시하고 싶다면

    private void Update()
    {
        // 1. 메인 퀘스트 업데이트
        UpdateMainQuestUI();

        // 2. 서브 퀘스트 업데이트 (선택 사항)
        UpdateSubQuestUI();
    }

    void UpdateMainQuestUI()
    {
        if (QuestManager.Instance == null) return;
        if (mainQuestNameText == null) return;

        var currentQuest = QuestManager.Instance.GetCurrentMainQuest();

        if (currentQuest != null)
        {
            if (currentQuest.state == QuestState.InProgress ||
                currentQuest.state == QuestState.ReadyToComplete)
            {
                // 퀘스트 이름(목표) 표시
                mainQuestNameText.text = currentQuest.questName;
            }
            else if (currentQuest.state == QuestState.Completed &&
                     QuestManager.Instance.GetCurrentMainQuest() == null) // 모든 메인 퀘스트 완료 시
            {
                mainQuestNameText.text = "모든 메인 퀘스트 완료";
            }
            else
            {
                // 진행 중인 퀘스트가 없을 때 빈 문자열 또는 대기 메시지
                mainQuestNameText.text = "";
            }
        }
        else
        {
            mainQuestNameText.text = "";
        }
    }

    // 서브 퀘스트는 진행 중인 모든 퀘스트를 표시할 수도 있지만, 
    // 예시에서는 하나의 서브 퀘스트만 검사하는 간단한 로직을 작성했습니다.
    void UpdateSubQuestUI()
    {
        if (QuestManager.Instance == null) return;
        if (subQuestNameText == null) return;

        // 예시: 첫 번째 서브 퀘스트(SQ1)만 표시
        var subQuest = QuestManager.Instance.GetSubQuestByID("SQ1");

        if (subQuest != null &&
            (subQuest.state == QuestState.InProgress || subQuest.state == QuestState.ReadyToComplete))
        {
            subQuestNameText.text = subQuest.questName;
        }
        else
        {
            subQuestNameText.text = "";
        }
    }
}
