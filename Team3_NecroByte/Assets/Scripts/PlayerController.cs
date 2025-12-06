
using System.Collections;
using TMPro;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    //¿òÁ÷ÀÓ °ü·Ã
    float hAxis;   //ÁÂ¿ì ¹æÇâÅ°
    float vAxis;   //»óÇÏ ¹æÇáÅ°
    bool rKey;     //´Þ¸®±â Å°
    bool dKey;     //È¸ÇÇ Å°
    bool eKey;     //»óÈ£ÀÛ¿ë Å°
    bool oneKey;   //1¹ø Å°
    bool twoKey;   //2¹ø Å°
    bool threeKey; //3¹ø Å°
    bool fourKey;  //4¹ø Å°
    bool fiveKey;  //5¹ø Å°
    bool aKey;     // °ø°Ý Å°
    bool lKey;     //ÀçÀåÀü Å°
    bool tKey;     //EMP ´øÁö´Â Å°

    [Header("ÇÃ·¹ÀÌ¾î ±âº» ¼³Á¤")]
    public float health;
    public int chip;
    public int Ammo;
    public int hasGrendes;
    public float maxHealth;
    public int maxChip;
    public int maxAmmo;
    public int MaxHasGrendes;

    [Header("ÀÌµ¿¼³Á¤")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 15f;
    private float currentSpeed;

    [Header("Ä¿Æ÷³ÍÆ®")]
    public Animator animator;
    public Animator pontAni;
    public Camera cam;
    private CharacterController controller;

    [Header("È¸ÇÇ ¼³Á¤")]
    public float dodgeDuration = 0.2f;          // È¸ÇÇ ÀüÃ¼ ½Ã°£
    public float dodgePower = 10f;              // ÃÖ´ë Èû
    public float dodgeCooldown = 0.4f;          
    public AnimationCurve dodgeCurve;           // ¼Óµµ Ä¿ºê

    public bool isDodgingNow = false;
    bool isDodging = false;
    bool dodgeOnCooldown =false;
    float dodgeTimer = 0f;
    Vector3 dodgeDirection;
    Vector3 dodgeVec;
  
    [Header("¹«±â ÀúÀå")]
    public GameObject[] weapons;
    public bool[] hasWeapons;
    GameObject nearObject;
    public Weapon equipWeapon;
    bool isSwap;
    int equipWeaponIndex = -1;
    // ÀçÀåÀü
    bool Reloading;                            
    // ±ÙÁ¢ °ø°Ý
    float fireDelay;
    bool isFireReady = true;
    bool isAttacking = false;
    float defaultRotationSpeed;  // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½â¸¦ ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ È¸ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½Ï°ï¿½
    // EMP 
    public GameObject GrenadeObj;

    [Header("Ä«¸Þ¶ó È¸Àü")]
    public Camera followCamera;

    [Header("UIÇ¥½Ã")]
    public TMP_Text healthText;
    public TMP_Text coinText;
    public RectTransform crosshair;

    // Áß·Â Ãß°¡
    float yVelocity = 0f;
    // Àû ÇÇ°Ý °ü·Ã
    bool isDamage;
    Renderer[] meshs;
    Color[] originalColors;

    void Start()
    { 
        controller = GetComponent<CharacterController>();
        UpdateUI();

        //Cursor.visible = false;
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
        Interaction();
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

       // ÇÃ·¹ÀÌ¾î°¡ ¹Ù¶óº¸´Â ¹æÇâ ±âÁØ ÀÌµ¿
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveVec = (forward * v) + (right * h);
        moveVec = moveVec.normalized;

        // È¸ÇÇ ÁßÀÌ¸é È¸ÇÇ º¤ÅÍ ¿ì¼±
        if (isDodging)
        {
            moveVec = dodgeVec;
        }

        // Áß·Â Ã³¸®
        if (!controller.isGrounded)
            yVelocity += Physics.gravity.y * Time.deltaTime;
        else
            yVelocity = -1f;

        // ¼Óµµ Àû¿ë
        float speed = (h != 0 || v != 0) ? (rKey ? runSpeed : walkSpeed) : 0;
        currentSpeed = speed;

        Vector3 finalMove = moveVec * currentSpeed + new Vector3(0, yVelocity, 0);
        controller.Move(finalMove * Time.deltaTime);

        // ¹«±â ±³Ã¼& °ø°Ý Áß ÀÌµ¿ Á¤Áö
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
        if (plane.Raycast(ray, out enter) && !isDead)
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
        if (!isDodging && !dodgeOnCooldown && !isSwap && dKey && !isDead)
        {
            dodgeDirection = transform.forward.normalized;
            isDodging = true;
            isDodgingNow = true;
            dodgeTimer = 0f;
            StartCoroutine(DodgeCooldownRoutine());
        }

        if (isDodging && !isDead)
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
        if (eKey && nearObject != null && !isDodging && !isSwap && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);

                Debug.Log("¾ÆÀÌÅÛ È¹µæ!");
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
  
        if (oneKey || twoKey || threeKey || fourKey && !isDodging && !isDead)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            Debug.Log("¹«±âÀåÂø!");
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
        healthText.text = $"{health}";
        coinText.text = $"{chip}";
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;
       
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(aKey && isFireReady && !isDodging && !isSwap && !isDead)
        {
            if (equipWeapon.type == Weapon.Type.Melee)
            {
                isAttacking = true;
                rotationSpeed = 0f;
                Invoke(nameof(EndAttack), 0.8f);
            }
            equipWeapon.UseWeapon();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "swingTrigger" : "shootTrigger");
           
            fireDelay = 0;
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        rotationSpeed = defaultRotationSpeed;
    }

    void Reload()
    {
        // Á¦ÇÑ»çÇ×
        if(equipWeapon == null) 
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (Ammo == 0)
            return;

        if(lKey && !isDodging && !isSwap && isFireReady && !isDead)
        {
            animator.SetTrigger("reloadTrigger");
            Reloading = true;

            Invoke("ReloadOut", 2f); //ÀçÀåÀü ¼Óµµ
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

        if(tKey && !isDodging && !isSwap && !Reloading && !isDead)
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

    void Interaction()
    {
        if (eKey && nearObject != null&& nearObject.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Enter(this);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.tag=="Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;

        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.EXIT();
            nearObject = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SceneObject>() != null)
        {
            string info = other.GetComponent<SceneObject>().objectInfo;
            if (info.StartsWith("scene"))
            {
                FadeManager.Instance.FadeOutAndLoadScene(info);
            }
        }
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Projectile enemyBullet = other.GetComponent<Projectile>();
                health -= enemyBullet.damage;
                UpdateUI();
                Debug.Log("µ¥¹ÌÁö!!");
                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach(Renderer mesh in meshs)
        {
            mesh.material.SetColor("_BaseColor", Color.red);
        }

        if (health <= 0 && !isDead)
        {
            
            OnDie();
        }

        //Time.timeScale = 0f;

        yield return new WaitForSeconds(1f);

        isDamage = false;
        for (int i = 0; i < meshs.Length; i++)
            meshs[i].material.SetColor("_BaseColor", originalColors[i]);
    }

    void OnDie()
    {
        animator.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();

    }



}
