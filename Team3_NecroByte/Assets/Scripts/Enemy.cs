using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { Normal, Rush, Fly };
    public Type enemyType;
    [Header("Enemy 정보")]
    public float maxHealth;
    public float curHealth;
    public Transform Target;
    public BoxCollider meleeArea;
    public bool isChase;
    public bool isAttack;
    public GameObject[] coins;

    [Header("비행체 정보")]
    public Transform model;           
    public float floatingHeight = 2f; 
    public Transform firePos;         
    public GameObject bulletPrefab;   
    public float fireDelay = 1.5f;    


    Rigidbody rigid;
    BoxCollider boxCollider;
    Renderer[] renderers;
    NavMeshAgent nav;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation;

        // Make the Rigidbody kinematic while NavMeshAgent controls movement to avoid sliding/pushing
        rigid.isKinematic = true;

        boxCollider = GetComponent<BoxCollider>();
        renderers = GetComponentsInChildren<Renderer>();
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

        if (rayHits.Length > 0 && !isAttack)
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
                yield return new WaitForSeconds(0.8f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(0.3f);
                break;
            case Type.Rush:
                yield return new WaitForSeconds(0.1f);
                // enable physics temporarily for the rush impulse
                rigid.isKinematic = false;
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                // disable physics again so NavMeshAgent regains smooth control
                rigid.isKinematic = true;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.Fly:
                yield return new WaitForSeconds(0.3f);
                firePos.rotation = Quaternion.LookRotation(model.forward);
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
        if (rb != null)
        {
            rb.useGravity = false;                     
            rb.velocity = firePos.forward * 20f;       
        }

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
        foreach (Renderer r in renderers)
            r.material.SetColor("_BaseColor", Color.red);

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (Renderer r in renderers)
                r.material.SetColor("_BaseColor", Color.white);
        }
        else
        {
            
            foreach (Renderer r in renderers)
                r.material.SetColor("_BaseColor", Color.gray);

            gameObject.layer = 9;
            isChase = false;
            nav.enabled = false;

            if (coins != null && coins.Length > 0)
            {
                int ranCoin = Random.Range(0, coins.Length); // use array length, not hard-coded 3
                GameObject coinPrefab = coins[ranCoin];
                if (coinPrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * 0.5f; // raise coin a little
                    Instantiate(coinPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning($"Enemy: coins[{ranCoin}] is null. Make sure coin prefabs are assigned in the Inspector.", this);
                }
            }
            else
            {
                Debug.LogWarning("Enemy: no coin prefabs assigned to 'coins' array.", this);
            }

            if (enemyType != Type.Fly)
            {
                anim.SetTrigger("DoDie");   
            }
            else
            {
                StartCoroutine(FlyDeathFall()); 
            }

            // ensure physics is enabled for the death knockback
            rigid.isKinematic = false;

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