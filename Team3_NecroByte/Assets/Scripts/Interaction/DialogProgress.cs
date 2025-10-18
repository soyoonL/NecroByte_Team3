using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DialogSystem;

public class DialogProgress : MonoBehaviour
{
    [SerializeField]
    public DialogSystem dialogSystem01;

    [SerializeField]
    public GameObject dialogObject;
    //public GameObject Page1;
    //public GameObject Page2;


    
    
    public IEnumerator Start()
    {


        //ù ��° ��� �б� ����
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());
        //��� �б� ���� ���ϴ� �ൿ �߰� ����
        //ĳ���͸� �����̰ų� �������� ȹ���ϴ� ����

         yield return new WaitWhile(() => dialogObject.activeSelf);
        gameObject.SetActive(false);
        //Page1.SetActive(false);
        //Page2.SetActive(false);




    }
}
