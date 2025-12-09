using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public Image fadeImage;
    public float fadeDuration = 2f;

    private void Start()
    {
        fadeImage.raycastTarget = false;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutRoutine(sceneName));
    }

    IEnumerator FadeOutRoutine(string sceneName)
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.StopControl(fadeDuration + 0.5f);
        }

        float timer = 0f;
        Color c = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = timer / fadeDuration;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);

        yield return null;

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        Color c = fadeImage.color;

        if (fadeDuration > 0)
        {
            float timer = 0f;
            c.a = 1f;
            fadeImage.color = c;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = 1f - (timer / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        c.a = 0f;
        fadeImage.color = c;
    }
}
