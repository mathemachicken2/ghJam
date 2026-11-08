
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    
    public GameObject deathPanel;

   
    public PlayerMovement playerMovement;

    private bool hasDied = false;

    private void Start()
    {
        deathPanel.SetActive(false);
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

        deathPanel.SetActive(true);
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
