using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    public Speaker[] speakers;          // ´ëÈ­¿¡ Âü¿©ÇÏ´Â Ä³¸¯ÅÍ UI ¹è¿­
    [SerializeField]
    public DialogData[] dialogs;        // ÇöÀç ºĞ±âÀÇ ´ë»ç ¸ñ·Ï ¹è¿­
    [SerializeField]
    public bool isAutoStart = true;     // ÀÚµ¿ ½ÃÀÛ ¿©ºÎ
    public bool isFirst = true;         // ÃÖÃÊ 1È¸¸¸ È£ÃâÇÏ±â À§ÇÑ º¯¼ö
    public int curDialogIndex = -1;     // ÇöÀç ´ë»ç ¼ø¹ø
    public int curSpeakerIndex = 0;     // ÇöÀç ¸»À» ÇÏ´Â È­ÀÚÀÇ speakers ¹è¿­ ¼ø


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
            Debug.Log("¾Æ´Ï¾Æ´Ï¾Æ¤ÑÇÏÆR´ÏÇÏ");
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

                    Debug.Log("µ¥È­³¡¤¼");
                   
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
    public struct Speaker //È­ÀÚÀÇ Á¤º¸¸¦ ÀúÀåÇÏ´Â ±¸Á¶Ã¼
    {
        public Image imageCharacter;      // Ä³¸¯ÅÍ ÀÌ¹ÌÁö
        public Image imageDialog;                  // ´ëÈ­Ã¢ ÀÌ¹ÌÁö UI
        public TextMeshProUGUI textName;           // ÇöÀç ´ë»çÁßÀÎ Ä³¸¯ÅÍ ÀÌ¸§ Ãâ·Â
        public TextMeshProUGUI textDialogue;       // ÇöÀç ´ë»ç Ãâ·Â
        public GameObject objectArrow;             // ´ë»ç°¡ ¿Ï·áµÇ¾úÀ» ¶§ Ãâ·ÂµÇ´Â Ä¿¼­ ¿ÀºêÁ§Æ®
    }

    [System.Serializable]
    public struct DialogData  //È­ÀÚ ÀÌ¸§ÀÌ¶û ´ë»ç Á¤º¸ ÀúÀå
    {
        public int speakerIndex;                  // ÀÌ¸§°ú ´ë»ç¸¦ Ãâ·ÂÇÒ ÇöÀç ´ÙÀÌ¾ó·Î±× ½Ã½ºÅÛÀÇ Speaker ¹è¿­ ¼ø¹ø
        public string name;                       // Ä³¸¯ÅÍ ÀÌ¸§
        [TextArea(3, 5)]
        public string dialogue;                   // ´ë»ç
    }


}
