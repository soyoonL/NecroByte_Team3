using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { Normal,Rush,Fly};
    public Type enemyType;
    [Header("기본정보")]
    public float maxHealth;
    public float curHealth;
    public Transform Target;
    public BoxCollider meleeArea;
    public bool isChase;
    public bool isAttack;

    [Header("C타입(비행 원거리)")]
    public Transform model;           // 적의 실제 모델
    public float floatingHeight = 2f; // 떠있는 높이
    public Transform firePos;         // 총알 발사 위치
    public GameObject bulletPrefab;   // 총알 프리팹
    public float fireDelay = 1.5f;    // 공격 간격
    

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation;

        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        if (enemyType != Type.Fly)
            anim.SetBool("isRun", true);
    }
    private void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(Target.position);
            nav.isStopped = !isChase;
        }

        if (enemyType == Type.Fly && model != null)
        {
            Vector3 pos = model.localPosition;
            pos.y = floatingHeight;
            model.localPosition = pos;

            // 플레이어 방향 보기 (쿼터뷰니까 Y 회전만)
            Vector3 look = Target.position;
            look.y = model.position.y;
            model.LookAt(look);
        }
    }


    void Targeting()
    {
        float targetRadius = 0f;
        float targetRange = 0f;

        switch (enemyType)
        {
            case Type.Normal:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.Rush:
                targetRadius = 1f;
                targetRange = 12f;
                break;
            case Type.Fly:
                targetRadius = 1.2f;
                targetRange = 10f;
                break;
        }

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if(rayHits.Length > 0 && !isAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false;    
        isAttack = true;

        if (enemyType != Type.Fly)
            anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.Normal:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.Rush:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20,ForceMode.Impulse);
                meleeArea.enabled=true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.Fly:
                yield return new WaitForSeconds(0.3f);
                FireBullet();
                yield return new WaitForSeconds(1f);
                break;
        }
        
        isChase = true;
        isAttack = false;
        if (enemyType != Type.Fly)
            anim.SetBool("isAttack", false);
    }

    private void FixedUpdate()
    {
        Targeting();
        
    }

    void FireBullet()
    {
        if (bulletPrefab == null || firePos == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePos.forward * 15f;   // 총알 속도
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec));
            Debug.Log("Melee : " + curHealth);
        }
        else if (other.tag == "Bullet")
        {
            Projectile projectile = other.GetComponent<Projectile>();
            curHealth -= projectile.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec));
            Debug.Log("Range : " + curHealth);
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec));
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.SetColor("_BaseColor", Color.red);
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.SetColor("_BaseColor", Color.white);
        }
        else
        {
            // 사망 처리
            mat.SetColor("_BaseColor", Color.gray);
            gameObject.layer = 9;
            isChase = false;
            nav.enabled = false;
            if (enemyType != Type.Fly)
                anim.SetTrigger("DoDie");

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 4, ForceMode.Impulse);

            Destroy(gameObject, 4);
        }
    }

    IEnumerator FlyDeathFall()
    {
        this.enabled = false;

        rigid.isKinematic = false;
        rigid.useGravity = true;

        float t = 0f;
        while (t < 1f)
        {
            model.Rotate(Vector3.forward * 300f * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
