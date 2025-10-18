using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    public Speaker[] speakers;          // 대화에 참여하는 캐릭터 UI 배열
    [SerializeField]
    public DialogData[] dialogs;        // 현재 분기의 대사 목록 배열
    [SerializeField]
    public bool isAutoStart = true;     // 자동 시작 여부
    public bool isFirst = true;         // 최초 1회만 호출하기 위한 변수
    public int curDialogIndex = -1;     // 현재 대사 순번
    public int curSpeakerIndex = 0;     // 현재 말을 하는 화자의 speakers 배열 순

    public void Awake()
    {
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
        if (isFirst == true)
        {
            SetUp();

            if (isAutoStart) SetNextDialog();

            isFirst = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
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
                }

                return true;
            }
        }

        return false;
    }

    public void SetNextDialog()
    {
        SetActiveObjects(speakers[curSpeakerIndex], false);

        curDialogIndex++;

        curSpeakerIndex = dialogs[curDialogIndex].speakerIndex;

        SetActiveObjects(speakers[curSpeakerIndex], true);
        speakers[curSpeakerIndex].textName.text = dialogs[curDialogIndex].name;
        speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue;
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
