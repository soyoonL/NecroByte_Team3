using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishQuestTrigger : MonoBehaviour
{
    [Header("이 트리거가 완료시킬 퀘스트 번호")]
    public int questNumber;

    [Header("한 번만 실행 여부")]
    public bool oneTime = true;

    bool isDone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isDone && oneTime) return;
        if (!other.CompareTag("Player")) return;

        // 퀘스트 완료
        QuestManager.Instance.CompleteCurrentMainQuest();

        isDone = true;
    }
}
