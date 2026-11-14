
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
   
    public GameObject menuPanel;
    public GameObject optionsPanel;

    
    public string gameSceneName = "SampleScene";

    
    public Image fadeImage;
    public float fadeDuration = 1.5f;
    public float whiteScreenDelay = 3f;


    public TMP_Text instructionText;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        fadeImage.gameObject.SetActive(true);

        Color color = Color.white;
        color.a = 0f;
        fadeImage.color = color;

        instructionText.gameObject.SetActive(false);
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

            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

       
        

       
        instructionText.gameObject.SetActive(true);
        instructionText.text = "Use SHIFT to rotate quickly";
        yield return new WaitForSeconds(whiteScreenDelay);

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
