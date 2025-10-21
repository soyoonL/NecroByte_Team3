using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
  public enum Type { Ammo, Coin, Grenade, HP, Weapon };
   
    public Type type;
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up*20*Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
