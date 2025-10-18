using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    public Speaker[] speakers;          // ��ȭ�� �����ϴ� ĳ���� UI �迭
    [SerializeField]
    public DialogData[] dialogs;        // ���� �б��� ��� ��� �迭
    [SerializeField]
    public bool isAutoStart = true;     // �ڵ� ���� ����
    public bool isFirst = true;         // ���� 1ȸ�� ȣ���ϱ� ���� ����
    public int curDialogIndex = -1;     // ���� ��� ����
    public int curSpeakerIndex = 0;     // ���� ���� �ϴ� ȭ���� speakers �迭 ��

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
