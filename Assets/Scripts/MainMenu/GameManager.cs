using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ESC Ayarları")]
    public float requiredHoldTime = 2f;
    private float escapeHoldTime = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            escapeHoldTime = 0f;
            return;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            escapeHoldTime += Time.deltaTime;

            if (escapeHoldTime >= requiredHoldTime)
            {
                escapeHoldTime = 0f;
                ReturnToMainMenu();
            }
        }
        else
        {
            escapeHoldTime = 0f;
        }
    }

    private void ReturnToMainMenu()
    {
        // UYARI DÜZELTİLDİ: FindObjectOfType yerine FindAnyObjectByType kullanıldı
        McMovement player = Object.FindAnyObjectByType<McMovement>();
        if (player != null)
        {
            PlayerPrefs.SetString("SavedLevel", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
            PlayerPrefs.Save();
            Debug.Log($"Oyun Kaydedildi! Sahne: {SceneManager.GetActiveScene().name} | Konum: X:{player.transform.position.x} Y:{player.transform.position.y}");
        }
        else
        {
            Debug.LogWarning("Sahnede McMovement bulunamadığı için konum kaydedilemedi!");
        }

        Debug.Log("Ana Menüye Dönülüyor...");

        if (SceneFader.instance != null)
        {
            SceneFader.instance.SahneDegistir("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu" && scene.name != "IntroScene")
        {
            if (PlayerPrefs.GetInt("LoadFromSave", 0) == 1)
            {
                // UYARI DÜZELTİLDİ: FindObjectOfType yerine FindAnyObjectByType kullanıldı
                McMovement player = Object.FindAnyObjectByType<McMovement>();
                if (player != null && PlayerPrefs.HasKey("PlayerPosX"))
                {
                    float savedX = PlayerPrefs.GetFloat("PlayerPosX");
                    float savedY = PlayerPrefs.GetFloat("PlayerPosY");

                    player.transform.position = new Vector3(savedX, savedY, player.transform.position.z);
                    Debug.Log("Oyuncu kayıtlı konuma yerleştirildi.");
                }

                PlayerPrefs.SetInt("LoadFromSave", 0);
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}