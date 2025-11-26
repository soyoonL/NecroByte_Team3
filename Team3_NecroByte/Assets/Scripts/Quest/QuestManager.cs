using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("메인퀘스트(순차 진행)")]
    public List<QuestData> mainQuests = new List<QuestData>();

    [Header("서브퀘스트(단발성)")]
    public List<QuestData> subQuests = new List<QuestData>();

    // 메인퀘스트의 현재 진행 인덱스 (런타임)
    private int currentMainIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // (선택) 에디터에서 세팅하지 않았으면 기본 샘플 세팅을 해둘 수 있다.
        if (mainQuests.Count == 0) CreateDefaultMainQuests();
        if (subQuests.Count == 0) CreateDefaultSubQuests();
    }

    #region --- 메인퀘스트 관련 헬퍼 ---
    public QuestData GetCurrentMainQuest()
    {
        if (currentMainIndex < 0 || currentMainIndex >= mainQuests.Count) return null;
        return mainQuests[currentMainIndex];
    }

    // 게임 시작과 함께 MQ1 자동 활성화 할 때 호출
    public void ActivateFirstMainQuest()
    {
        currentMainIndex = 0;
        var q = GetCurrentMainQuest();
        Debug.Log("시작!");
        if (q != null && q.state == QuestState.NotStarted)
            q.state = QuestState.InProgress;
    }

    // 메인퀘스트 직접 수락 (대사 후 호출)
    public bool AcceptMainQuest(string questID)
    {
        var q = mainQuests.FirstOrDefault(x => x.questID == questID);
        if (q == null) return false;
        if (q.state != QuestState.NotStarted) return false;
        q.state = QuestState.InProgress;
        return true;
    }

    // 메인 퀘스트 완료 처리 (트리거/대화에서 호출)
    public bool CompleteCurrentMainQuest()
    {
        var q = GetCurrentMainQuest();
        if (q == null) return false;
        if (q.state == QuestState.InProgress || q.state == QuestState.ReadyToComplete)
        {
            q.state = QuestState.Completed;
            Debug.Log($"[QuestManager] 메인퀘스트 완료: {q.questID} - {q.questName}");
            // 보상 지급은 여기서 처리하거나 NPC 스크립트에서 처리해도 된다.
            AdvanceMainIndex();
            return true;
        }
        return false;
    }

    private void AdvanceMainIndex()
    {
        currentMainIndex++;
        if (currentMainIndex < mainQuests.Count)
        {
            var next = mainQuests[currentMainIndex];
            if (next.state == QuestState.NotStarted)
                next.state = QuestState.InProgress; // 다음 퀘스트 자동 활성화 (요구사항에 따라 변경 가능)
            Debug.Log($"[QuestManager] 다음 메인퀘스트 활성화: {next.questID}");
        }
        else
        {
            Debug.Log("[QuestManager] 모든 메인퀘스트 완료!");
        }
    }
    #endregion

    #region --- 서브퀘스트 관련 ---
    public QuestData GetSubQuestByID(string id)
    {
        return subQuests.FirstOrDefault(x => x.questID == id);
    }

    public bool AcceptSubQuest(string questID)
    {
        var q = GetSubQuestByID(questID);
        if (q == null) return false;
        if (q.state != QuestState.NotStarted) return false;
        q.state = QuestState.InProgress;
        return true;
    }

    public bool CompleteSubQuest(string questID)
    {
        var q = GetSubQuestByID(questID);
        if (q == null) return false;
        if (q.state == QuestState.InProgress || q.state == QuestState.ReadyToComplete)
        {
            q.state = QuestState.Completed;
            Debug.Log($"[QuestManager] 서브퀘스트 완료: {q.questID}");
            return true;
        }
        return false;
    }
    #endregion

    #region --- 아이템 관련 (아이템 획득 시 호출) ---
    // itemID를 통해 해당 퀘스트(메인 또는 서브)가 item을 요구하는지 검사
    public bool IsItemRequiredByAnyInProgressQuest(string itemID)
    {
        // 메인 (현재 진행중인 메인만 체크)
        var main = GetCurrentMainQuest();
        if (main != null && main.IsInProgress() && main.requireItem && main.requiredItemID == itemID)
            return true;

        // 서브(진행중인 모든 서브퀘 체크)
        foreach (var s in subQuests)
            if (s.IsInProgress() && s.requireItem && s.requiredItemID == itemID)
                return true;

        return false;
    }

    // 아이템을 획득했을 때 호출. (아이템을 바로 삭제하거나 플레이어 인벤토리에 넣은 뒤 호출)
    // 여기서는 '획득' => 퀘스트 상태를 ReadyToComplete로 바꿔서 NPC에게 돌아가면 완료할 수 있게 한다.
    public void MarkItemCollectedForQuests(string itemID)
    {
        var main = GetCurrentMainQuest();
        if (main != null && main.IsInProgress() && main.requireItem && main.requiredItemID == itemID)
        {
            main.state = QuestState.ReadyToComplete;
            Debug.Log($"[QuestManager] 메인퀘스트 아이템 획득: {main.questID}");
        }

        foreach (var s in subQuests)
        {
            if (s.IsInProgress() && s.requireItem && s.requiredItemID == itemID)
            {
                s.state = QuestState.ReadyToComplete;
                Debug.Log($"[QuestManager] 서브퀘스트 아이템 획득: {s.questID}");
            }
        }
    }
    #endregion

    #region --- 완성된 예제 데이터 (에디터에서 세팅하는걸 권장) ---
    private void CreateDefaultMainQuests()
    {
        // MQ1
        var a = ScriptableObject.CreateInstance<QuestData>();
        a.questID = "MQ1";
        a.questName = "대피소로 가기";
        a.isMainQuest = true;
        a.requireItem = false;
        a.rewardGold = 0;
        a.state = QuestState.NotStarted;
        mainQuests.Add(a);

        // MQ2
        var b = ScriptableObject.CreateInstance<QuestData>();
        b.questID = "MQ2";
        b.questName = "사람들과 대화하기";
        b.isMainQuest = true;
        b.requireItem = false;
        b.rewardGold = 0;
        b.state = QuestState.NotStarted;
        mainQuests.Add(b);

        // MQ3 (칩 필요)
        var c = ScriptableObject.CreateInstance<QuestData>();
        c.questID = "MQ3";
        c.questName = "데이터베이스 칩 찾기";
        c.isMainQuest = true;
        c.requireItem = true;
        c.requiredItemID = "DataChip"; // 네가 말한 데이터베이스 칩 아이디
        c.rewardGold = 100;
        c.state = QuestState.NotStarted;
        mainQuests.Add(c);

        // MQ4
        var d = ScriptableObject.CreateInstance<QuestData>();
        d.questID = "MQ4";
        d.questName = "지하철 탑승 준비";
        d.isMainQuest = true;
        d.requireItem = false;
        d.rewardGold = 200;
        d.state = QuestState.NotStarted;
        mainQuests.Add(d);
    }

    private void CreateDefaultSubQuests()
    {
        var s = ScriptableObject.CreateInstance<QuestData>();
        s.questID = "SQ1";
        s.questName = "옛날 사진 찾아주기";
        s.isMainQuest = false;
        s.requireItem = true;
        s.requiredItemID = "OldPhoto";
        s.rewardGold = 50;
        s.state = QuestState.NotStarted;
        subQuests.Add(s);
    }
    #endregion

    #region --- 유틸 / 디버그 ---
    // 퀘스트 상태 출력 (디버깅용)
    public void PrintAllQuestStates()
    {
        Debug.Log("=== Quest States ===");
        for (int i = 0; i < mainQuests.Count; i++)
        {
            var q = mainQuests[i];
            Debug.Log($"Main[{i}] {q.questID} : {q.state}");
        }
        foreach (var s in subQuests)
            Debug.Log($"Sub {s.questID} : {s.state}");
    }
    #endregion
}
