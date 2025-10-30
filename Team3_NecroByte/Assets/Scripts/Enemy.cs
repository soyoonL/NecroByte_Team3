using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth;
    public float curHealth;

    public float moveSpeed = 2f; // �̵��ӵ�
    public float traceRange = 15f;
    public bool isDead = false;
    private Transform player;  //�÷��̾� ����

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    private void Update()
    {
        TracePlayer();
    }

    private void TracePlayer()
    {

        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if ( distance <= traceRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            transform.LookAt(player.position);
        }
  
    }
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
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

        else if(other.tag == "Bullet")
        {
            Projectile projectile = other.GetComponent<Projectile>();
            curHealth -= projectile.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec));
            Debug.Log("Range : " + curHealth);
        }

        IEnumerator OnDamage(Vector3 reactVec)
        {
            mat.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            if (curHealth > 0)
            {
                mat.color = Color.white;
            }
            else
            {
                mat.color = Color.gray;
                gameObject.layer = 9;

                isDead = true;
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 4, ForceMode.Impulse);

                Destroy(gameObject, 4);
            }
        }
    }
}
