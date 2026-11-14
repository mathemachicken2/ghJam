
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ambient Sounds")]
    public AudioClip ambientSoundMenu;
    public AudioClip ambientSoundScene;

    [Header("Sound Effects")]
    public AudioClip enemySpawnSound;
    public AudioClip deathByEnemySound;
    public AudioClip deathByTrapSound;
    public AudioClip eatingSound;

    private AudioSource ambientSource;
    private AudioSource sfxSource;

    public float menuAmbientVolume = 0.8f;

    [Range(0f, 1f)]
    public float sceneAmbientVolume = 0.35f;

    [Header("SFX Volume")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    public string menuSceneName = "MainMenu";
    public string gameSceneName = "SampleScene";

    private void Awake()
    {
        // Prevent duplicate AudioManagers
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.spatialBlend = 0f;

        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        ambientSource.volume = menuAmbientVolume;
        sfxSource.volume = sfxVolume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlaySceneAmbient(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneAmbient(scene.name);
    }

    private void PlaySceneAmbient(string sceneName)
    {
        AudioClip newClip = null;
        float newVolume = 0.5f;

        if (sceneName == menuSceneName)
        {
            newClip = ambientSoundMenu;
            newVolume = menuAmbientVolume;
        }
        else if (sceneName == gameSceneName)
        {
            newClip = ambientSoundScene;
            newVolume = sceneAmbientVolume;
        }

        if (ambientSource.clip == newClip)
            return;

        ambientSource.Stop();
        ambientSource.clip = newClip;
        ambientSource.volume = newVolume;

        if (newClip != null)
            ambientSource.Play();
    }

    // SOUND EFFECTS

    public void PlayEnemySpawn()
    {
        PlaySFX(enemySpawnSound);
    }

    public void PlayDeathByEnemy()
    {
        PlaySFX(deathByEnemySound);
    }

    public void PlayDeathByTrap()
    {
        PlaySFX(deathByTrapSound);
    }

    public void PlayEating()
    {
        PlaySFX(eatingSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
