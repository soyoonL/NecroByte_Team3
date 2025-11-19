using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("기본정보")]
    public float maxHealth;
    public float curHealth;
    public Transform Target;
    public BoxCollider meleeArea;
    public bool isChase;
    public bool isAttack;
    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isRun", true);
    }
    private void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(Target.position);
            nav.isStopped = !isChase;
        }  
    }


    void Targeting()
    {
        float targetRadius = 1.5f;
        float targetRange = 3f;

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
        anim.SetBool("isAttack", true);

        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(1f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    private void FixedUpdate()
    {
        Targeting();
        
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
            anim.SetTrigger("DoDie");

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 4, ForceMode.Impulse);

            Destroy(gameObject, 4);
        }
    }
}
