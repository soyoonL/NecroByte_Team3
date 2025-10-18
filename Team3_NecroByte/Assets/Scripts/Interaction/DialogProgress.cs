using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogProgress : MonoBehaviour
{
    [SerializeField]
    private DialogSystem dialogSystem01;
    
    private IEnumerator Start()
    {
        
        //ù ��° ��� �б� ����
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());
        //��� �б� ���� ���ϴ� �ൿ �߰� ����
        //ĳ���͸� �����̰ų� �������� ȹ���ϴ� ����
        
    }
}
