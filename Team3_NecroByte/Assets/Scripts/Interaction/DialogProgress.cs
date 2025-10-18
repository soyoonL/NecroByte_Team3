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
        
        //첫 번째 대사 분기 시작
        yield return new WaitUntil(() => dialogSystem01.UpdateDialog());
        //대사 분기 사이 원하는 행동 추가 가능
        //캐릭터를 움직이거나 아이템을 획득하는 등의
        
    }
}
