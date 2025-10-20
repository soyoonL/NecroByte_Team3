using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    public Speaker[] speakers;            // ��ȭ�� �����ϴ� ĳ���� UI �迭
    [SerializeField]
    public DialogData[] dialogs;          // ���� �б��� ��� ��� �迭
    [SerializeField]
    public bool isAutoStart = true;       // �ڵ� ���� ����
    public bool isFirst = true;           // ���� 1ȸ�� ȣ���ϱ� ���� ����
    public int curDialogIndex = -1;       // ���� ��� ����
    public int curSpeakerIndex = 0;       // ���� ���� �ϴ� ȭ���� speakers �迭 ��
    private float typingSpeed = 0.1f;     // �ؽ�Ʈ Ÿ���� ȿ���� ��� �ӵ�
    private bool isTypingEffect = false;  // �ؽ�Ʈ Ÿ���� ȿ���� ���������


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
            // �ؽ�Ʈ Ÿ���� ȿ�� ��� ���� ��, ���콺 ���� Ŭ���ϸ� Ÿ���� ȿ�� ����
            if(isTypingEffect == true)
            {
                isTypingEffect = false;

                // Ÿ���� ȿ���� �����ϰ�, ���� ��� ��ü�� ���
                StopCoroutine("OnTypingText");
                speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue;
                // ��簡 �Ϸ�Ǿ��� ��, ��µǴ� Ŀ�� Ȱ��ȭ
                speakers[curSpeakerIndex].objectArrow.SetActive(true);

                return false;
            }
            
            // ��簡 �������� ��� ���� ��� ����
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

                    Debug.Log("��ȭ����");
                   
                }
                
                return true;
            }
        }

        return false;
    }

    public void SetNextDialog()
    {
        // ���� ȭ���� ��ȭ ���� ������Ʈ ��Ȱ��ȭ
        SetActiveObjects(speakers[curSpeakerIndex], false);

        // ���� ��� ����
        curDialogIndex++;

        // ���� ȭ�� ���� ����
        curSpeakerIndex = dialogs[curDialogIndex].speakerIndex;

        // ���� ȭ�� ��ȭ ���� ������Ʈ Ȱ��ȭ
        SetActiveObjects(speakers[curSpeakerIndex], true);

        // ���� ȭ�� �̸� �ؽ�Ʈ ����
        speakers[curSpeakerIndex].textName.text = dialogs[curDialogIndex].name;

        // ���� ȭ���� ��� �ؽ�Ʈ ����
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

        // �ؽ�Ʈ�� �ѱ��ھ� Ÿ����ġ�� ���
        while (index <= dialogs[curDialogIndex].dialogue.Length)
        {
            speakers[curSpeakerIndex].textDialogue.text = dialogs[curDialogIndex].dialogue.Substring(0, index);

            index++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingEffect = false;

        // ��簡 �Ϸ�Ǿ��� �� ��µǴ� Ŀ�� Ȱ��ȭ
        speakers[curSpeakerIndex].objectArrow.SetActive(true);
    }

    [System.Serializable]
    public struct Speaker //ȭ���� ������ �����ϴ� ����ü
    {
        public Image imageCharacter;      // ĳ���� �̹���
        public Image imageDialog;                  // ��ȭâ �̹��� UI
        public TextMeshProUGUI textName;           // ���� ������� ĳ���� �̸� ���
        public TextMeshProUGUI textDialogue;       // ���� ��� ���
        public GameObject objectArrow;             // ��簡 �Ϸ�Ǿ��� �� ��µǴ� Ŀ�� ������Ʈ
    }

    [System.Serializable]
    public struct DialogData  //ȭ�� �̸��̶� ��� ���� ����
    {
        public int speakerIndex;                  // �̸��� ��縦 ����� ���� ���̾�α� �ý����� Speaker �迭 ����
        public string name;                       // ĳ���� �̸�
        [TextArea(3, 5)]
        public string dialogue;                   // ���
    }


}
