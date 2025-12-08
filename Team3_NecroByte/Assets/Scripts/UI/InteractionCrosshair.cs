using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionCrosshair : MonoBehaviour
{
    [SerializeField] Camera cam;

    RaycastHit hitInfo;

    [SerializeField] GameObject NormalCrosshair;
    [SerializeField] GameObject EnemyCrosshair;

    [SerializeField] Canvas uiCanvas;

    bool isContact = false;

    void Update()
    {
        UpdateCrosshairPosition();
        CheckObject();
    }

    void UpdateCrosshairPosition()
    {
        Vector3 mousePos = Input.mousePosition;

        if (uiCanvas == null)
        {
            // For Screen Space - Overlay canvases or world-space GameObjects used as "cursor"
            if (NormalCrosshair) NormalCrosshair.transform.position = mousePos;
            if (EnemyCrosshair) EnemyCrosshair.transform.position = mousePos;
        }
        else
        {
            // For Canvas set to Screen Space - Camera or World Space, convert screen point
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransform normalRT = NormalCrosshair ? NormalCrosshair.GetComponent<RectTransform>() : null;
            RectTransform enemyRT = EnemyCrosshair ? EnemyCrosshair.GetComponent<RectTransform>() : null;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera, out localPoint))
            {
                if (normalRT) normalRT.anchoredPosition = localPoint;
                if (enemyRT) enemyRT.anchoredPosition = localPoint;
            }
        }
    }

    void CheckObject()
    {
        // Use Input.mousePosition directly for the raycast
        Vector3 t_MousePos = Input.mousePosition;

        if (cam == null)
            cam = Camera.main;

        if (Physics.Raycast(cam.ScreenPointToRay(t_MousePos), out hitInfo, 100f))
        {
            Contact();
        }
        else
        {
            NotContact();
        }
            
    }

    void Contact()
    {
        if (hitInfo.transform.CompareTag("Enemy"))
        {
            if (!isContact)
            {
                isContact = true;
                if (EnemyCrosshair) EnemyCrosshair.SetActive(true);
                if (NormalCrosshair) NormalCrosshair.SetActive(false);
            }
        }
        else
        {
            NotContact();
        }
    }

    void NotContact()
    {
        if (isContact)
        {
            isContact = false;
            if (EnemyCrosshair) EnemyCrosshair.SetActive(false);
            if (NormalCrosshair) NormalCrosshair.SetActive(true);
        }
           
    }
}
