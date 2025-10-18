using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

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
                TalkToNPC();
                break;
        }
    }

    protected virtual void CollectItem() //������ ���� �Լ�
    {
        
        Destroy(gameObject);    // ������ �ı�
        
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
        dialogSystem.SetActive( true );
        dialogProgress.SetActive( true );
        dialogPage1.SetActive( true );
        dialogPage2.SetActive( true );  
      
    }
}
