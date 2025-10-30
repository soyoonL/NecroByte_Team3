using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFollowAI : MonoBehaviour
{
    public Transform player;  // �÷��̾��� Transform
    public float followSpeed = 5f;  // ������� �ӵ�
    public float heightOffset = 2f; //�÷��̾� �������� ������ ����
    public float followDistance = 2f;   // �÷��̾���� �Ÿ� ����
    public float smoothness = 3f;  //������ �ε巴��

    private Vector3 targetPos;

    private void Update()
    {
        if (player == null) return;

        targetPos = player.position + (- player.forward * followDistance) + (Vector3.up * heightOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);

        transform.LookAt(player.position + Vector3.up * 1.5f);
       
    }
}
