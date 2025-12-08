using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameCam;
    public GameObject overPanel;
    public PlayerController player;
    //public Enemy enemy;
    public int stage;
    public bool isBattle;
    //public int enemyA;
    //public int enemyB;
    //public int enemyC;

    public GameObject gamePanel;
    public TextMeshProUGUI playerHPtxt;
    public TextMeshProUGUI playerAmmotxt;
    public TextMeshProUGUI playerCointxt;
    public TextMeshProUGUI playerGranade;

    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weapon4Img;
    public Image weapon5Img;

    public Image curweapon1Img;
    public Image curweapon2Img;
    public Image curweapon3Img;
    public Image curweapon4Img;
    
    //public RectTransform EnemyHP;
    //public RectTransform EnemyHPBar;

    private void LateUpdate()
    {
        playerHPtxt.text = $"{player.health}";
        playerCointxt.text = $"{player.chip}";
        playerAmmotxt.text = $"{player.Ammo}";
        playerGranade.text = $"{player.hasGrendes}";

        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weapon4Img.color = new Color(1, 1, 1, player.hasWeapons[3] ? 1 : 0);
        weapon5Img.color = new Color(1, 1, 1, player.hasGrendes > 0 ? 1f : 0f);

        curweapon1Img.color = new Color(1, 1, 1, player.equipWeaponIndex == 0 ? 1 : 0);
        curweapon2Img.color = new Color(1, 1, 1, player.equipWeaponIndex == 1 ? 1 : 0);
        curweapon3Img.color = new Color(1, 1, 1, player.equipWeaponIndex == 2 ? 1 : 0);
        curweapon4Img.color = new Color(1, 1, 1, player.equipWeaponIndex == 3 ? 1 : 0);

    }

    public void GameOver()
    {
        overPanel.SetActive(true);
        gamePanel.SetActive(false);
    }
}
