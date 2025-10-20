using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    public Speaker[] speakers;            // 대화에 참여하는 캐릭터 UI 배열
    [SerializeField]
    public DialogData[] dialogs;          // 현재 분기의 대사 목록 배열
    [SerializeField]
    public bool isAutoStart = true;       // 자동 시작 여부
    public bool isFirst = true;           // 최초 1회만 호출하기 위한 변수
    public int curDialogIndex = -1;       // 현재 대사 순번
    public int curSpeakerIndex = 0;       // 현재 말을 하는 화자의 speakers 배열 순
    private float typingSpeed = 0.1f;     // 텍스트 타이핑 효과의 재생 속도
    private bool isTypingEffect = false;  // 텍스트 타이핑 효과를 재생중인지


    public void Awake()
    {
        SetUp(); 
    }

    public void OnEnable()
    {
        isAutoStart = true;
        isFirst = true;
        curDialogIndex = -1;
        curSpeakerIndex = 0;
        SetUp();
    }

    public void SetUp()
    {
        for ( int i = 0; i<speakers.Length; ++i)
        {
            SetActiveObjects(speakers[i], false);
            speakers[i].imageCharacter.gameObject.SetActive(true);
            
        }
    }

    public bool UpdateDialog()
    {
        if(isFirst == true)
        {
            SetUp();

            if (isAutoStart) SetNextDialog();

            isFirst = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 텍스트 타이핑 효과 재생 중일 때, 마우스 왼쪽 클릭하면 타이핑 효과 종료
            if(isTypingEffect == true)
            {
                isTypingEffect = false;

                // 타이핑 효과를 중지하고, 현재 대사 전체를 출력
                StopCoroutine("OnTypingText");
                speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue;
                // 대사가 완료되었을 때, 출력되는 커서 활성화
                speakers[curSpeakerIndex].objectArrow.SetActive(true);

                return false;
            }
            
            // 대사가 남아있을 경우 다음 대사 진행
            if ( dialogs.Length> curDialogIndex + 1)
            {
                SetNextDialog();
            }

            else
            {
                for ( int i = 0; i < speakers.Length; ++i)
                {
                    SetActiveObjects(speakers[i], false);
                    speakers[i].imageCharacter.gameObject.SetActive(false);
                    gameObject.SetActive(false);

                    Debug.Log("데화끝ㅌ");
                   
                }
                
                return true;
            }
        }

        return false;
    }

    public void SetNextDialog()
    {
        // 이전 화자의 대화 관련 오브젝트 비활성화
        SetActiveObjects(speakers[curSpeakerIndex], false);

        // 다음 대사 진행
        curDialogIndex++;

        // 현재 화자 순번 설정
        curSpeakerIndex = dialogs[curDialogIndex].speakerIndex;

        // 현재 화자 대화 관련 오브젝트 활성화
        SetActiveObjects(speakers[curSpeakerIndex], true);

        // 현재 화자 이름 텍스트 설정
        speakers[curSpeakerIndex].textName.text = dialogs[curDialogIndex].name;

        // 현재 화자의 대사 텍스트 설정
        //speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue;
        StartCoroutine("OnTypingText");
    }

    public void SetActiveObjects(Speaker speaker, bool visible)
    {
        speaker.imageDialog.gameObject.SetActive(visible);
        speaker.textName.gameObject.SetActive(visible);
        speaker.textDialogue.gameObject.SetActive(visible);

        speaker.objectArrow.SetActive(false) ;

        Color color = speaker.imageCharacter.color;
        color.a = visible == true ? 1 : 0.2f;
        speaker.imageCharacter.color = color;
    }

    private IEnumerator OnTypingText()
    {
        int index = 0;

        isTypingEffect = true;

        // 텍스트를 한글자씩 타이핑치듯 재생
        while (index <= dialogs[curDialogIndex].dialogue.Length)
        {
            speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue.Substring(0, index);

            index++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingEffect = false;

        // 대사가 완료되었을 때 출력되는 커서 활성화
        speakers[curSpeakerIndex].objectArrow.SetActive(true);
    }

    [System.Serializable]
    public struct Speaker //화자의 정보를 저장하는 구조체
    {
        public Image imageCharacter;      // 캐릭터 이미지
        public Image imageDialog;                  // 대화창 이미지 UI
        public TextMeshProUGUI textName;           // 현재 대사중인 캐릭터 이름 출력
        public TextMeshProUGUI textDialogue;       // 현재 대사 출력
        public GameObject objectArrow;             // 대사가 완료되었을 때 출력되는 커서 오브젝트
    }

    [System.Serializable]
    public struct DialogData  //화자 이름이랑 대사 정보 저장
    {
        public int speakerIndex;                  // 이름과 대사를 출력할 현재 다이얼로그 시스템의 Speaker 배열 순번
        public string name;                       // 캐릭터 이름
        [TextArea(3, 5)]
        public string dialogue;                   // 대사
    }


}
