
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    [Header("Death UI")]
    public GameObject deathPanel;
    public Image fadeImage;

    [Header("Fade Settings")]
    public float fadeDelay = 2f;
    public float fadeDuration = 1.5f;

    public PlayerMovement playerMovement;

    private bool hasDied = false;

    private void Start()
    {
        deathPanel.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);

            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    public void Die()
    {
        if (hasDied)
            return;

        hasDied = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);

            deathPanel.SetActive(true);

            yield return new WaitForSeconds(fadeDelay);

            float elapsed = 0f;

            

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;

                float alpha = Mathf.Clamp01(elapsed / fadeDuration);

                Color color = fadeImage.color;
                color.a = alpha;
                fadeImage.color = color;

                yield return null;
            }

           
            Color finalColor = fadeImage.color;
            finalColor.a = 1f;
            fadeImage.color = finalColor;
        }

        
       
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
