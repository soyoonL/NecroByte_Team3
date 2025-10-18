using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    [Header("��ȣ�ۿ� ����")]
    public float interactionRange = 2.0f; //��ȣ�ۿ� ����
    public LayerMask interactionLayerMask = 1; //��ȣ�ۿ��� ���̾�
    public KeyCode interactionKey = KeyCode.E; //��ȣ�ۿ� Ű

    [Header("UI ����")]
    public Text interactionText; //��ȣ�ۿ� UI �ؽ�Ʈ
    public GameObject interactionUI; //��ȣ�ۿ� UI �г�

    private Transform playerTransform; 
    private InteractableObject curInteractiable;  // ������ ������Ʈ ��� Ŭ����

    private void Start()
    {
       playerTransform = transform;
       HideInteractionUI();
    }

    private void Update()
    {
        CheckForInteractables();
        HandleInteractionInput();
    }

    void HandleInteractionInput()
    {
        if(curInteractiable != null && Input.GetKeyDown(interactionKey)) //������ ������Ʈ�� �ְ� eŰ�� ������ Ŭ������ �ִ� Interact �Լ��� ����
        {
            curInteractiable.Interact();
           
        }
      
    }

    void ShowInteractionUI(string text)
    {
        if (interactionUI != null)      //UI â�� �����ϸ� ȭ�鿡 ǥ��
        {
            interactionUI.SetActive(true); 
        }

        if (interactionText != null)   //�ؽ�Ʈ�� �����ϸ� ���ڿ��� �ؽ�Ʈ ����
        {
            interactionText.text = text;
        }
    }

    void HideInteractionUI() 
    {
        if (interactionUI != null)  //UIâ�� �����ϸ� â�� ����
        {
            interactionUI.SetActive(false);
        }
    }

    void CheckForInteractables()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (interactionRange * 0.5f);
        //�÷��̾� ���� ��ġ + �÷��̾ ���� �������� interactionRange * 0.5f �Ÿ���ŭ ���� ���� ���

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, interactionRange, interactionLayerMask);

        InteractableObject closestInteractable = null;   //���� ����� ��ü ����
        float closestDistance = float.MaxValue;  //�Ÿ� ����

        foreach (Collider collider in hitColliders)
        {
            InteractableObject interactable = collider.GetComponent<InteractableObject>(); //������Ʈ�� �ش� ������Ʈ�� ������ �ִ���
            if (interactable != null) //��ȣ�ۿ� ����
            {

                float distance = Vector3.Distance(playerTransform.position, collider.transform.position); //�÷��̾�� ������Ʈ ���� �Ÿ� ���

                //�÷��̾ �ٶ󺸴� ���⿡ �ִ��� Ȯ�� (����üũ)
                Vector3 directionToObject = (collider.transform.position - playerTransform.position).normalized;
                float angle = Vector3.Angle(playerTransform.forward, directionToObject);

                if (angle <90f && distance < closestDistance) // �þ߰� 50�� �̳�, ���� ����� �繰�� ���
                {
                    closestDistance = distance;
                    closestInteractable = interactable;  //���� ����� ��ȣ�ۿ����� ����
                }

            }
        }

        if (closestInteractable != curInteractiable)
        {
            if(curInteractiable != null)
            {
                curInteractiable.OnPlayerExit();
            }

            curInteractiable = closestInteractable;

            if (curInteractiable != null)
            {
                curInteractiable.OnPlayerEnter(); //�� ������Ʈ ����
                ShowInteractionUI(curInteractiable.GetInteractionText());
            }
            else
            {
                HideInteractionUI();
            }
        }

    }
}
