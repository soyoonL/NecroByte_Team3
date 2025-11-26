using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum QuestState
{
    NotStarted,
    InProgress,
    ReadyToComplete, // 아이템 요구 퀘스트의 경우 아이템을 획득했지만 NPC 전달 전 상태
    Completed
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Quests/QuestData", order = 0)]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questID;        // ex "MQ1", "MQ2", "SQ1"
    public string questName;
    public bool isMainQuest = false;

    [Header("조건 (필요시에만)")]
    public bool requireItem = false;   // 아이템을 요구하는 퀘스트면 true
    public string requiredItemID;      // 예: "DataChip", "OldPhoto"

    [Header("보상")]
    public int rewardGold = 0;

    [Header("상태 (런타임에서 변경)")]
    public QuestState state = QuestState.NotStarted;

    // 유틸
    public bool IsAvailableToAccept()
    {
        return state == QuestState.NotStarted;
    }

    public bool IsInProgress()
    {
        return state == QuestState.InProgress || state == QuestState.ReadyToComplete;
    }
}
