using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    //������ ����
    float hAxis; //�¿� ����Ű
    float vAxis; //���� ����Ű
    bool rKey; //�޸��� Ű
    bool dKey; //ȸ�� Ű
    bool eKey; //��ȣ�ۿ� Ű
    bool oneKey; //1�� Ű
    bool twoKey; //2�� Ű
    bool fourKey; //4�� Ű
    bool fiveKey; //5�� Ű
    bool aKey; // ���� Ű
    bool lKey; //������ Ű
    bool tKey; //EMP ������ Ű
    bool cKey; // ī�޶� ȸ�� Ű

    [Header("�̵�����")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 10.0f;

    private float currentSpeed;

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

    Vector3 moveVec;
    Vector3 dodgeVec;
  
    [Header("���� ����")]
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject throwObj;

    //������ ����
    GameObject nearObject;
    Weapon equipWeapon;
    bool isSwap;
    int equipWeaponIndex = -1;

    //����
    float fireDelay;
    bool isFireReady;

    [Header("�Ѿ� ��")]
    public int Bullet;
    public int maxBullet;

    //���콺 ȸ��
    [Header("ī�޶� ȸ��")]
    public Camera followCamera;

    //������
    bool Reloading;

    //�߷� �߰�
    float yVelocity = 0f;

   
    void Start()
    {
        controller = GetComponent<CharacterController>();

    }


    void Update()
    {

        GetInput();
        HandleMovement();
        UpdateAnimator();
        HandleDodge();
        Interation();
        Swap();
        Attack();
        Reload();
        Turn();
        

    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        rKey = Input.GetKey(KeyCode.LeftShift);
        dKey = Input.GetKey(KeyCode.Space);
        eKey = Input.GetKeyDown(KeyCode.E);
        oneKey = Input.GetKeyDown(KeyCode.Alpha1);
        twoKey = Input.GetKeyDown(KeyCode.Alpha2);
        fourKey = Input.GetKeyDown(KeyCode.Alpha4);
        fiveKey = Input.GetKeyDown(KeyCode.Alpha5);
        aKey = Input.GetMouseButtonDown(0);
        lKey = Input.GetKeyDown(KeyCode.Q);
        tKey = Input.GetMouseButtonDown(1);
        cKey = Input.GetMouseButtonDown(2);
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(aKey && isFireReady && !isDodging && !isSwap)
        {
            equipWeapon.UseWeapon();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "swingTrigger" : "shootTrigger");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if(equipWeapon == null) 
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (Bullet == 0)
            return;

        if(lKey && !isDodging && !isSwap && isFireReady)
        {
            animator.SetTrigger("reloadTrigger");
            Reloading = true;

            Invoke("ReloadOut", 2f); //������ �ӵ�
        }
    }

    void ReloadOut()
    {
        int reBullet = Bullet < equipWeapon.maxBullet ? Bullet : equipWeapon.maxBullet;
        equipWeapon.curBullet = reBullet;
        Reloading = false;
        Bullet -= reBullet;
    }

    void HandleMovement()
    {

        float h = hAxis;
        float v = vAxis;
        Vector3 moveVec = new Vector3(h, 0, v).normalized;

        // �߷� ���� �߰�
        if (!controller.isGrounded)
        {
            yVelocity += Physics.gravity.y * Time.deltaTime; //���� ������� ���� �� �߷�����
        }
        else
        {
            // ���� ������ �ӵ��� ����
            yVelocity = -1f;
        }

        //���� �̵� ���� ���
        Vector3 finalMove = (moveVec * currentSpeed) + new Vector3(0, yVelocity, 0);
        controller.Move(finalMove * Time.deltaTime);

        if(isSwap || Reloading ) 
            moveVec = Vector3.zero;  //��ȣ�ۿ��� �� ������X

        if (isDodging)
        {
            moveVec = dodgeVec;
        }

        if (h != 0 || v != 0)
        {


            if (rKey)
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveVec), rotationSpeed * Time.deltaTime);  //ž�信���� �ڿ������� ������ȯ
        }
    }

    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("speed", animatorSpeed);

    }

    void HandleDodge()
    {

        if (dKey && !isDodging && Time.time - lastDodgeTime >= dodgeCooldown&&!isSwap)
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
            nearObject = other.gameObject;

       
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }

    void Interation()
    {
        if(eKey && nearObject != null)
        {
           
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);

                Debug.Log("������ ȹ��!");
            }
        }
    }

    void Swap()
    {

        if (oneKey && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (twoKey && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (fourKey && (!hasWeapons[3]|| equipWeaponIndex == 3))
            return;
        if (fiveKey && (!hasWeapons[4] || equipWeaponIndex == 4))
            return;

        int weaponIndex = -1;
        if (oneKey) weaponIndex = 0;
        if (twoKey) weaponIndex = 1;
        if (fourKey) weaponIndex = 3;
        if (fiveKey) weaponIndex = 4;
       

        if (oneKey || twoKey || fourKey || fiveKey && !isDodging )
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            Debug.Log("��������!");
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("swapTrigger");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
        
        
    }
    void SwapOut()
    {
        isSwap = false;
    }

    void Turn()
    {
        // Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);

        // ���콺�� ���� ȸ��
        if (cKey)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y= 0;   
                transform.LookAt(transform.position + nextVec);
            }
        }
       
    }

   
}
