using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using static DialogSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("��ȣ�ۿ� ����")]
    public string objectName = "������";
    public string interactionText = "[E] ��ȣ�ۿ�";
    public InteractionType interactionType = InteractionType.Item;

    [Header("���̶���Ʈ ����")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;

    public Renderer objectRenderer;
    private Color originColor;
    private bool isHighlighted = false;

    [Header("��ȭ ����")]
    public GameObject dialogSystem;
    public GameObject dialogProgress;
    public GameObject dialogPage1;
    public GameObject dialogPage2;

    DialogSystem diaSystem;
    


    public enum InteractionType
    {
        Item,  //������
        Machine, //���
        Buiding, //�ǹ�
        NPC,  //NPC
        Cellectible  //����ǰ
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

    public virtual void OnPlayerEnter() //�÷��̾ ������ ������ ���̶���Ʈ ����
    {
        Debug.Log($"[{objectName}] ������");
        HighlightObject();
    }

    public virtual void OnPlayerExit() //�÷��̾ �������� ����� ������� ���ƿ�
    {
        Debug.Log($"[{objectName}] �������� ���");
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

    public string GetInteractionText() //UI�� ������ Text ���� �Լ�
    {
        return interactionText;
    }

    public virtual void Interact()
    {
        
        switch (interactionType) //��ȣ�ۿ뿡 ���� �⺻����
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

    protected virtual void CollectItem() //������ ���� �Լ�
    {
        Destroy(gameObject);    // ������ �ı�

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

    protected virtual void OperateMachine() //��� �۵� �Լ�
    {
        if(objectRenderer != null)
        {
            objectRenderer.material.color = Color.green; //���� �ʷϻ�����
        }
    }

    protected virtual void AccessBuilding() //���� ����
    {
        transform.Rotate(Vector3.up * 90f); //ȸ��
    }

    protected virtual void TalkToNPC() //NPC ��ȭ
    {
        Debug.Log($"{objectName}�� ��ȭ�� �����մϴ�."); //�켱 ����׷θ�
       

      

    }
}
