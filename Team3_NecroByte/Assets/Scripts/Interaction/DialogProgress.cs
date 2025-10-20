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

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(StartDialogProgress());
    }



    public IEnumerator StartDialogProgress()
    {
        //ù ��° ��� �б� ����
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());
        //��� �б� ���� ���ϴ� �ൿ �߰� ����
        //ĳ���͸� �����̰ų� �������� ȹ���ϴ� ����

         yield return new WaitWhile(() => dialogObject.activeSelf);
         gameObject.SetActive(false);
       
    }
}
