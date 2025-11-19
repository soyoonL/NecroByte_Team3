
using System.Collections;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    //움직임 관련
    float hAxis;   //좌우 방향키
    float vAxis;   //상하 방햐키
    bool rKey;     //달리기 키
    bool dKey;     //회피 키
    bool eKey;     //상호작용 키
    bool oneKey;   //1번 키
    bool twoKey;   //2번 키
    bool threeKey; //3번 키
    bool fourKey;  //4번 키
    bool fiveKey;  //5번 키
    bool aKey;     // 공격 키
    bool lKey;     //재장전 키
    bool tKey;     //EMP 던지는 키

    [Header("플레이어 기본 설정")]
    public float health;
    public int chip;
    public int Ammo;
    public int hasGrendes;
    public float maxHealth;
    public int maxChip;
    public int maxAmmo;
    public int MaxHasGrendes;

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
    public float dodgePower = 10f;              // 최대 힘
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
    GameObject nearObject;
    Weapon equipWeapon;
    bool isSwap;
    int equipWeaponIndex = -1;
    // 재장전
    bool Reloading;                            
    // 근접 공격
    float fireDelay;
    bool isFireReady = true;
    // EMP 
    public GameObject GrenadeObj;

    [Header("카메라 회전")]
    public Camera followCamera;

    [Header("UI표시")]
    public TMP_Text healthText;
    public TMP_Text coinText;
    public RectTransform crosshair;

    // 중력 추가
    float yVelocity = 0f;
    // 적 피격 관련
    bool isDamage;
    Renderer[] meshs;
    Color[] originalColors;

    void Start()
    { 
        controller = GetComponent<CharacterController>();
        UpdateUI();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Awake()
    {
        meshs = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshs.Length];

        for (int i = 0; i < meshs.Length; i++)
        {
            originalColors[i] = meshs[i].material.GetColor("_BaseColor");
        }
    }
    void Update()
    {
        crosshair.position = Input.mousePosition;

        GetInput();
        HandleMovement();
        UpdateAnimator();
        Dodge();
        Interation();
        Swap();
        Attack();
        Reload();
        Grenade();
        TurnWithCrosshair();
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
        threeKey = Input.GetKeyDown(KeyCode.Alpha3);
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

       // 플레이어가 바라보는 방향 기준 이동
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveVec = (forward * v) + (right * h);
        moveVec = moveVec.normalized;

        // 회피 중이면 회피 벡터 우선
        if (isDodging)
        {
            moveVec = dodgeVec;
        }

        // 중력 처리
        if (!controller.isGrounded)
            yVelocity += Physics.gravity.y * Time.deltaTime;
        else
            yVelocity = -1f;

        // 속도 적용
        float speed = (h != 0 || v != 0) ? (rKey ? runSpeed : walkSpeed) : 0;
        currentSpeed = speed;

        Vector3 finalMove = moveVec * currentSpeed + new Vector3(0, yVelocity, 0);
        controller.Move(finalMove * Time.deltaTime);

        // 무기 교체& 공격 중 이동 정지
        if (isSwap || Reloading || !isFireReady)
            moveVec = Vector3.zero;
    }

    void UpdateAnimator()
    {
        float animatorSpeed = Mathf.Clamp01(currentSpeed / runSpeed);
        animator.SetFloat("speed", animatorSpeed);

    }

    void TurnWithCrosshair()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(crosshair.position);
        Plane plane = new Plane(Vector3.up, transform.position);

        float enter;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 dir = hitPoint - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
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

            controller.Move(dodgeDirection * power * Time.deltaTime);

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
        if (threeKey && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;
        if (fourKey && (!hasWeapons[3] || equipWeaponIndex == 3))
            return;

        int weaponIndex = -1;
        if (oneKey) weaponIndex = 0;
        if (twoKey) weaponIndex = 1;
        if (threeKey) weaponIndex = 2;
        if (fourKey) weaponIndex = 3;
  
        if (oneKey || twoKey || threeKey || fourKey  && !isDodging)
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
        // 제한사항
        if(equipWeapon == null) 
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (Ammo == 0)
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
        int reAmmo = Ammo < equipWeapon.maxAmmo ? Ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        Reloading = false;
        Ammo -= reAmmo;
    }

    void Grenade()
    {
        if (hasGrendes == 0)
            return;

        if(tKey  && !isDodging && !isSwap && !Reloading)
        {
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(crosshair.position);
            Plane plane = new Plane(Vector3.up, transform.position);

            float enter;
            if (plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                Vector3 dir = hitPoint - transform.position;
                dir.y = 0;

                GameObject instantGrenade = Instantiate(GrenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGre = instantGrenade.GetComponent<Rigidbody>();
                rigidGre.AddForce(dir, ForceMode.Impulse);
                rigidGre.AddTorque(Vector3.back*1, ForceMode.Impulse);

                hasGrendes--;

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
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Projectile enemyBullet = other.GetComponent<Projectile>();
                health -= enemyBullet.damage;
                Debug.Log("데미지!!");
                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach(Renderer mesh in meshs)
        {
            mesh.material.SetColor("_BaseColor", Color.yellow);
        }

        yield return new WaitForSeconds(1f);

        isDamage = false;
        for (int i = 0; i < meshs.Length; i++)
            meshs[i].material.SetColor("_BaseColor", originalColors[i]);
    }



}
