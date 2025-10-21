using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    //움직임 관련
    float hAxis; //좌우 방향키
    float vAxis; //상하 방햐키
    bool rKey; //달리기 키
    bool dKey; //회피 키
    bool eKey; //상호작용 키
    bool oneKey; //1번 키
    bool twoKey; //2번 키
    bool fourKey; //4번 키
    bool fiveKey; //5번 키
    bool aKey; // 공격 키
    bool lKey; //재장전 키
    bool tKey; //EMP 던지는 키
    bool cKey; // 카메라 회전 키

    [Header("이동설정")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 10.0f;

    private float currentSpeed;

    [Header("커포넌트")]
    public Animator animator;

    private CharacterController controller;
    private Camera playerCamera;

    [Header("회피 설정")]
    public float dodgeDistance = 2.5f;      // 이동 거리
    public float dodgeDuration = 0.25f;     // 이동 시간 (짧을수록 순간 이동 느낌)
    public float dodgeCooldown = 0.5f;

    private bool isDodging = false;
    private float lastDodgeTime = 0f;
    private Vector3 dodgeStartPos;
    private Vector3 dodgeEndPos;
    private float dodgeTimer;

    Vector3 moveVec;
    Vector3 dodgeVec;
  
    [Header("무기 저장")]
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject throwObj;

    //아이템 저장
    GameObject nearObject;
    Weapon equipWeapon;
    bool isSwap;
    int equipWeaponIndex = -1;

    //공격
    float fireDelay;
    bool isFireReady;

    [Header("총알 수")]
    public int Bullet;
    public int maxBullet;

    //마우스 회전
    [Header("카메라 회전")]
    public Camera followCamera;

    //재장전
    bool Reloading;

    //중력 추가
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

            Invoke("ReloadOut", 2f); //재장전 속도
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

        // 중력 변수 추가
        if (!controller.isGrounded)
        {
            yVelocity += Physics.gravity.y * Time.deltaTime; //땅에 닿아있지 않을 때 중력적용
        }
        else
        {
            // 땅에 닿으면 속도를 리셋
            yVelocity = -1f;
        }

        //최종 이동 벡터 계산
        Vector3 finalMove = (moveVec * currentSpeed) + new Vector3(0, yVelocity, 0);
        controller.Move(finalMove * Time.deltaTime);

        if(isSwap || Reloading ) 
            moveVec = Vector3.zero;  //상호작용할 때 움직임X

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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveVec), rotationSpeed * Time.deltaTime);  //탑뷰에서도 자연스럽게 방향전환
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

            // 시작/끝 위치 계산
            dodgeStartPos = transform.position;
            dodgeEndPos = transform.position + transform.forward * dodgeDistance;
        }

        // 회피 중이면 부드럽게 이동
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

                Debug.Log("아이템 획득!");
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
            Debug.Log("무기장착!");
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
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        // 마우스에 의한 회전
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
