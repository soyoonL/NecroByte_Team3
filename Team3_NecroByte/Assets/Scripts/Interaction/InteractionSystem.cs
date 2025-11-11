using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    [Header("상호작용 설정")]
    public float interactionRange = 2.0f;          // 상호작용 범위
    public LayerMask interactionLayerMask = 1;     // 상호작용할 레이어
    public KeyCode interactionKey = KeyCode.E;     // 상호작용 키

    [Header("UI 설정")]
    public Text interactionText;                   // 상호작용 UI 텍스트
    public GameObject interactionUI;               // 상호작용 UI 패널

    private Transform playerTransform; 
    private InteractableObject curInteractiable;   // 감지된 오브젝트 담는 클래스

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
        if(curInteractiable != null && Input.GetKeyDown(interactionKey)) // 참조할 오브젝트가 있고 e키를 누르면 클래스에 있는 Interact 함수를 실행
        {
            curInteractiable.Interact();
           
        }
      
    }

    void ShowInteractionUI(string text)
    {
        if (interactionUI != null)           //UI 창이 존재하면 화면에 표시
        {
            interactionUI.SetActive(true); 
        }

        if (interactionText != null)         //텍스트가 존재하면 문자열로 텍스트 변경
        {
            interactionText.text = text;
        }
    }

    void HideInteractionUI() 
    {
        if (interactionUI != null)           //UI창이 존재하면 창을 닫음
        {
            interactionUI.SetActive(false);
        }
    }

    void CheckForInteractables()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (interactionRange * 0.5f);
        //플레이어 현재 위치 + 플레이어가 보는 방향으로 interactionRange * 0.5f 거리만큼 앞쪽 지점 계산

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, interactionRange, interactionLayerMask);

        InteractableObject closestInteractable = null;   //가장 가까운 물체 선언
        float closestDistance = float.MaxValue;          //거리 설정

        foreach (Collider collider in hitColliders)
        {
            InteractableObject interactable = collider.GetComponent<InteractableObject>(); //오브젝트가 해당 컴포넌트를 가지고 있는지
            if (interactable != null) //상호작용 가능
            {

                float distance = Vector3.Distance(playerTransform.position, collider.transform.position); //플레이어와 오브젝트 사이 거리 계산

                //플레이어가 바라보는 방향에 있는지 확인 (각도체크)
                Vector3 directionToObject = (collider.transform.position - playerTransform.position).normalized;
                float angle = Vector3.Angle(playerTransform.forward, directionToObject);

                if (angle <90f && distance < closestDistance) // 시야각 50도 이내, 가장 가까운 사물일 경우
                {
                    closestDistance = distance;
                    closestInteractable = interactable;  //가장 가까운 상호작용으로 저장
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
                curInteractiable.OnPlayerEnter(); //새 오브젝트 선택
                ShowInteractionUI(curInteractiable.GetInteractionText());
            }
            else
            {
                HideInteractionUI();
            }
        }

    }
}
