using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    public GameObject creditsPanel; // Emeği Geçenler görselini/panelini buraya atayacağız

    void Update()
    {
        // Eğer Credits paneli açıksa ve ESC (Escape) tuşuna basılırsa
        if (creditsPanel != null && creditsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCredits(); // Paneli kapat
        }
    }

    public void PlayGame()
    {
        SceneFader.instance.SahneDegistir("IntroScene");
    }

    public void ContinueGame()
    {
        // GameManager'a oyunun bir kayıttan yüklendiğini bildiriyoruz (Kritik Adım!)
        PlayerPrefs.SetInt("LoadFromSave", 1);

        // Kaydedilen sahne adını string olarak alıyoruz (Varsayılan olarak "Level1" dedik, ilk bölümünün adı neyse onu yazabilirsin)
        string savedSceneName = PlayerPrefs.GetString("SavedLevel", "Level1");

        // O sahneyi yüklüyoruz
        SceneManager.LoadScene(savedSceneName);
    }

    // Emeği Geçenler butonuna basıldığında çalışacak
    public void ShowCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true); // Görseli/Paneli aktif et
        }
    }

    // ESC tuşuna basıldığında çalışacak (İstersen panele bir "Kapat" butonu ekleyip bu fonksiyonu ona da bağlayabilirsin)
    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false); // Görseli/Paneli gizle
        }
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");

        // Gerçek oyunda (Build alındıktan sonra) uygulamayı kapatır
        Application.Quit();

        // Unity Editöründe test ederken Play modunu durdurur
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}