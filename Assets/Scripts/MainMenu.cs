
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

    [Header("Scene")]
    public string gameSceneName = "SampleScene";

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // Start with a transparent white overlay
        fadeImage.gameObject.SetActive(true);

        Color color = Color.white;
        color.a = 0f;
        fadeImage.color = color;
    }

    public void PlayGame()
    {
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsed = 0f;

        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(
                0f,
                1f,
                elapsed / fadeDuration
            );

            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
