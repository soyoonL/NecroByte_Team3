using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }
    public Type type;
    public int damage;
    public float rate;                   // 총알 발사하는 속도
    public int maxAmmo;
    public int curAmmo;
    [Header("근접공격")]
    public BoxCollider meleeArea;        //공격범위
    public TrailRenderer trailEffect;    //휘두를 때의 효과
    [Header("원거리공격")]
    public Camera cam;
    public Transform bulletPos;          // 프리팹을 생성하는 위치
    public GameObject bullet;            // 프리팹을 저장할 변수
    public Transform bulletCasePos;
    public GameObject bulletCase;

   public void UseWeapon()
   {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && curAmmo > 0) 
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
  
   }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        //2
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = false;
        //3
        yield return new WaitForSeconds(0.15f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        //총알 발사
        GameObject instantBullet = Instantiate (bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 shootDir;

        //PlayerController player = GetComponent<PlayerController>();
        //player.Ammo--;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            shootDir = (hit.point - bulletPos.position).normalized;
        }
        else
        {
            shootDir = ray.direction;
        }

        bulletRigid.velocity = shootDir * 50f;  // 총알 날라가는 속도

        yield return null;
        //탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody CaseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletPos.up* Random.Range(2,3) + Vector3.right* Random.Range(-3,-2);   //총알이 튕겨나가듯이 (탄피에 랜덤한 힘 가하기)
        CaseRigid.AddForce(caseVec,ForceMode.Impulse);
        CaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}
