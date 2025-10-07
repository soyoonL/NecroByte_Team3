using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    [Header("�̵�����")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 10.0f;


    [Header("Ŀ����Ʈ")]
    public Animator animator;

    private CharacterController controller;
    private Camera playerCamera;

    [Header("ȸ�� ����")]
    public float dodgeDistance = 2.5f;      // �̵� �Ÿ�
    public float dodgeDuration = 0.25f;     // �̵� �ð� (ª������ ���� �̵� ����)
    public float dodgeCooldown = 0.5f;

    private bool isDodging = false;
    private float lastDodgeTime = 0f;
    private Vector3 dodgeStartPos;
    private Vector3 dodgeEndPos;
    private float dodgeTimer;

    private float currentSpeed;

    Vector3 moveVec;
    Vector3 dodgeVec;

    //������ ����
    GameObject saveObject;
    GameObject equipWeapon;
    
    [Header("���� ����")]
    public GameObject[] weapons;
    public bool[] hasWeapons;

    void Start()
    {
        controller = GetComponent<CharacterController>();

    }


    void Update()
    {

        HandleMovement();
        UpdateAnimator();
        HandleDodge();
        Interation();
        ChangeWeapons();

    }

    void HandleMovement()
    {

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveVec = new Vector3(h, 0, v).normalized;

        controller.Move(moveVec * currentSpeed * Time.deltaTime);

        if (isDodging)
        {
            moveVec = dodgeVec;
        }

        if (h != 0 || v != 0)
        {


            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = runSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }


        }
        else
        {
            currentSpeed = 0;
        }

        if (moveVec != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveVec), rotationSpeed * Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("speed", animatorSpeed);

    }

    void HandleDodge()
    {

        if (Input.GetKeyDown(KeyCode.Space) && !isDodging && Time.time - lastDodgeTime >= dodgeCooldown)
        {
            isDodging = true;
            lastDodgeTime = Time.time;
            dodgeTimer = 0f;
            currentSpeed *= 2f;
            
            animator.ResetTrigger("dodgeTrigger");
            animator.SetTrigger("dodgeTrigger");

            // ����/�� ��ġ ���
            dodgeStartPos = transform.position;
            dodgeEndPos = transform.position + transform.forward * dodgeDistance;
        }

        // ȸ�� ���̸� �ε巴�� �̵�
        if (isDodging)
        {
            dodgeTimer += Time.deltaTime;
            float t = dodgeTimer / dodgeDuration;
            if (t > 1f) t = 1f;

            
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            Vector3 newPos = Vector3.Lerp(dodgeStartPos, dodgeEndPos, smoothT);
            controller.Move(newPos - transform.position);

            
            if (t >= 1f)
            {
                isDodging = false;
            }
        }


    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag=="Weapon")
            saveObject = other.gameObject;

        Debug.Log(saveObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            saveObject = null;
    }

    void Interation()
    {
        if(Input.GetKeyDown(KeyCode.E) && saveObject != null)
        {
           
            if (saveObject.tag == "Weapon")
            {
                Item item = saveObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(saveObject);

                Debug.Log("������ ȹ��!");
            }
        }
    }

    void ChangeWeapons()
    {
        int weaponIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponIndex = 1;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) && !isDodging )
        {
            if (equipWeapon != null) equipWeapon.SetActive(false);

            equipWeapon = weapons[weaponIndex];
            Debug.Log("��������!");
            equipWeapon.SetActive(true);
        }
        
        
    }
}
