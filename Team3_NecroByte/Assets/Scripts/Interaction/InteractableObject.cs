using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using static DialogSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("상호작용 정보")]
    public string objectName = "아이템";
    public string interactionText = "[E] 상호작용";
    public InteractionType interactionType = InteractionType.Item;

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;

    public Renderer objectRenderer;
    private Color originColor;
    private bool isHighlighted = false;

    [Header("대화 설정")]
    public GameObject dialogSystem;
    public GameObject dialogProgress;
    public GameObject dialogPage1;
    public GameObject dialogPage2;

    DialogSystem diaSystem;
    


    public enum InteractionType
    {
        Item,  //아이템
        Machine, //기계
        Buiding, //건물
        NPC,  //NPC
        Cellectible  //수집품
    }

    protected virtual void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if(objectRenderer != null)
        {
            originColor = objectRenderer.material.color;
        }
        gameObject.layer = 10;
    }

    public virtual void OnPlayerEnter() //플레이어가 범위에 들어오면 하이라이트 들어옴
    {
        Debug.Log($"[{objectName}] 감지됨");
        HighlightObject();
    }

    public virtual void OnPlayerExit() //플레이어가 범위에서 벗어나면 원래대로 돌아옴
    {
        Debug.Log($"[{objectName}] 범위에서 벗어남");
        RemoveHighlight();
    }
    protected virtual void HighlightObject()
    {
        if (objectRenderer != null && !isHighlighted)
        {
            objectRenderer.material.color = highlightColor;
            objectRenderer.material.SetFloat("_Emission", highlightIntensity);
            isHighlighted = true;
        }
    }

    protected virtual void RemoveHighlight()
    {
        if (objectRenderer != null && isHighlighted)
        {
            objectRenderer.material.color = originColor;
            objectRenderer.material.SetFloat("_Emission", 0f);
            isHighlighted = false;
        }
    }

    public string GetInteractionText() //UI에 보여줄 Text 리턴 함수
    {
        return interactionText;
    }

    public virtual void Interact()
    {
        
        switch (interactionType) //상호작용에 따른 기본동작
        {
            
            case InteractionType.Item:
                CollectItem();    
                break;  
            case InteractionType.Machine:
                OperateMachine();
                break;
            case InteractionType.Buiding:
                AccessBuilding();
                break;
            case InteractionType.Cellectible:
                CollectItem();
                break;
            case InteractionType.NPC:
                Debug.Log(2222222222);
                TalkToNPC();
                dialogSystem.SetActive(true);
                dialogProgress.SetActive(true);
                dialogPage1.SetActive(true);
                dialogPage2.SetActive(true);
                break;
        }
    }

    protected virtual void CollectItem() //아이템 수집 함수
    {
        Destroy(gameObject);    // 아이템 파괴

        Item item = GetComponent<Item>();
        GameObject player = GameObject.Find("Player");
        PlayerController pc= player.GetComponent<PlayerController>();

        switch (item.type)
        {
            case Item.Type.Coin:
                pc.chip += item.value;
                pc.UpdateUI();
                if (pc.chip > pc.maxChip)
                    pc.chip = pc.maxChip;
                break;
            case Item.Type.HP:
                pc.health += item.value;
                pc.UpdateUI();
                if(pc.health > pc.maxHealth)    
                    pc.health = pc.maxHealth;
                break;
            case Item.Type.Grenade:
                pc.hasthrow += item.value;
                if(pc.hasthrow > pc.MaxHasThrow)
                    pc.hasthrow = pc.MaxHasThrow;
                break;

        }

        
    }

    protected virtual void OperateMachine() //기계 작동 함수
    {
        if(objectRenderer != null)
        {
            objectRenderer.material.color = Color.green; //색을 초록색으로
        }
    }

    protected virtual void AccessBuilding() //빌딩 접근
    {
        transform.Rotate(Vector3.up * 90f); //회전
    }

    protected virtual void TalkToNPC() //NPC 대화
    {
        Debug.Log($"{objectName}와 대화를 시작합니다."); //우선 디버그로만
       

      

    }
}
