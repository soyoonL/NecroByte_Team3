using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPosition : MonoBehaviour
{
    void LateUpdate()
    {
        PlayerController.Instance.transform.position = transform.position;
        Destroy(gameObject);
    }

}
