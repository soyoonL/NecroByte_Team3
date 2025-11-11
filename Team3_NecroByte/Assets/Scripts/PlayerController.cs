
using System.Collections;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    //움직임 관련
    float hAxis;  //좌우 방향키
    float vAxis;  //상하 방햐키
    bool rKey;    //달리기 키
    bool dKey;    //회피 키
    bool eKey;    //상호작용 키
    bool oneKey;  //1번 키
    bool twoKey;  //2번 키
    bool fourKey; //4번 키
    bool fiveKey; //5번 키
    bool aKey;    // 공격 키
    bool lKey;    //재장전 키
    bool tKey;    //EMP 던지는 키

    [Header("플레이어 기본 설정")]
    public float health;
    public int chip;
    public int Bullet;
    public int hasthrow;
    public float maxHealth;
    public int maxChip;
    public int maxBullet;
    public int MaxHasThrow;

    [Header("이동설정")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 15f;
    private float currentSpeed;

    [Header("커포넌트")]
    public Animator animator;
    public Camera cam;
    private CharacterController controller;

    [Header("회피 설정")]
    public float dodgeDuration = 0.2f;          // 회피 전체 시간
    public float dodgePower = 70f;              // 최대 힘
    public float dodgeCooldown = 0.4f;          
    public AnimationCurve dodgeCurve;           // 속도 커브

    public bool isDodgingNow = false;
    bool isDodging = false;
    bool dodgeOnCooldown =false;
    float dodgeTimer = 0f;
    Vector3 dodgeDirection;
    Vector3 dodgeVec;
  
    [Header("무기 저장")]
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject throwObj;
    GameObject nearObject;
    Weapon equipWeapon;
    bool isSwap;
    int equipWeaponIndex = -1;

    //공격
    float fireDelay;
    bool isFireReady;
    public bool isAttacking;

    //마우스 회전
    [Header("카메라 회전")]
    public Camera followCamera;

    [Header("UI표시")]
    public TMP_Text healthText;
    public TMP_Text coinText;

    //재장전
    bool Reloading;

    //중력 추가
    float yVelocity = 0f;

    void Start()
    { 
        controller = GetComponent<CharacterController>();
        UpdateUI();
    }

    void Update()
    {

        GetInput();
        HandleMovement();
        UpdateAnimator();
        Dodge();
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
    }

    void HandleMovement()
    {
        float h = hAxis;
        float v = vAxis;

        Vector3 camForward = cam.transform.forward;                                    // 카메라 기준 방향
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveVec = (camForward * v + camRight * h).normalized;                  // 카메라 기준 이동 벡터

        if (isDodging)                                                                 // 회피 중이면 회피 방향 우선
        {
            moveVec = dodgeVec;
        }

        if (!controller.isGrounded)                                                    // 중력 처리
            yVelocity += Physics.gravity.y * Time.deltaTime;
        else
            yVelocity = -1f;

        float speed = rKey ? runSpeed : walkSpeed;                                     // 최종 이동 속도 적용
        if (h == 0 && v == 0) speed = 0;

        currentSpeed = speed;

        Vector3 finalMove = moveVec * currentSpeed + new Vector3(0, yVelocity, 0);
        controller.Move(finalMove * Time.deltaTime);

        if (isSwap || Reloading || isAttacking)                                        // 무기 교체, 공격 중 움직임 막기
            moveVec = Vector3.zero;
    }

    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("speed", animatorSpeed);

    }

    void Turn()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);                          // 마우스가 향하는 곳을 레이캐스트

        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0)); // 캐릭터가 서 있는 평면

        float enter;

        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 direction = hitPoint - transform.position;                        // 방향 계산
            direction.y = 0;

            if (direction.sqrMagnitude < 0.05f)                                       // 너무 가까우면 방향이 튀므로 보정
                return;

            Quaternion targetRot = Quaternion.LookRotation(direction);                  
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime); // 실제 회전
        }
    }

    void Dodge()
    {
        if (!isDodging && !dodgeOnCooldown && !isSwap && dKey)
        {
            dodgeDirection = transform.forward.normalized;
            isDodging = true;
            isDodgingNow = true;
            dodgeTimer = 0f;
            StartCoroutine(DodgeCooldownRoutine());
        }

        if (isDodging)
        {
            dodgeTimer += Time.deltaTime;
            float t = dodgeTimer / dodgeDuration;

            float power = dodgeCurve.Evaluate(t) * dodgePower;

            transform.position += dodgeDirection * power * Time.deltaTime;

            if (t >= 1f)
            {
                isDodging = false;
                isDodgingNow = false;
            }
        }
    }

    IEnumerator DodgeCooldownRoutine()
    {
        dodgeOnCooldown = true;
        yield return new WaitForSeconds(dodgeCooldown);
        dodgeOnCooldown = false;
    }

    void Interation()
    {
        if (eKey && nearObject != null && !isDodging && !isSwap)
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
        if (fourKey && (!hasWeapons[3] || equipWeaponIndex == 3))
            return;
        if (fiveKey && (!hasWeapons[4] || equipWeaponIndex == 4))
            return;

        int weaponIndex = -1;
        if (oneKey) weaponIndex = 0;
        if (twoKey) weaponIndex = 1;
        if (fourKey) weaponIndex = 3;
        if (fiveKey) weaponIndex = 4;


        if (oneKey || twoKey || fourKey || fiveKey && !isDodging)
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

    public void UpdateUI()
    {
        healthText.text = $"{health} / {maxHealth}";
        coinText.text = $"Chip : {chip}";
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

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SceneObject>() != null)
        {
            string info = other.GetComponent<SceneObject>().objectInfo;
            if (info.StartsWith("scene"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(info);
            }
        }
    }



}
