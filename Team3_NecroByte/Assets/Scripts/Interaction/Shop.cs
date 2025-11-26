using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    PlayerController enterPlayer;

    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;

    public void Enter(PlayerController player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

   
    public void EXIT()
    {
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.chip)
        {
            return;
        }

        enterPlayer.chip -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);

        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);
    }
}
