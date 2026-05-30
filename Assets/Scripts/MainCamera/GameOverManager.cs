using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject gameOverUI;
    public VideoPlayer videoPlayer;
    public CanvasGroup uiCanvasGroup;
    public Image siyahKapanisEkrani;

    [Header("Görünürlük Kontrolü")]
    public Camera mainCamera;
    public Renderer playerRenderer;

    [Header("Geçiş Ayarları")]
    public float acilisSuresi = 1.5f;
    public float kapanisSuresi = 1.0f;
    public float kameraDisiTetiklemeSuresi = 1.0f;

    [Tooltip("Intro bitene kadar Game Over sisteminin çalışmasını engeller.")]
    public float baslangicGecikmesi = 5.5f;

    private bool isDead = false;
    private float _kameraDisiSure = 0f;
    private float _oyunSuresi = 0f;

    void Start()
    {
        AudioListener.pause = false;

        // Video varsa bitiş eventini dinle
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;

        if (mainCamera == null) mainCamera = Camera.main;

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }

        if (siyahKapanisEkrani != null)
        {
            siyahKapanisEkrani.color = new Color(0, 0, 0, 0);
            siyahKapanisEkrani.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isDead || playerRenderer == null) return;

        _oyunSuresi += Time.deltaTime;
        if (_oyunSuresi < baslangicGecikmesi) return;

        if (IsPlayerOutsideCamera())
        {
            _kameraDisiSure += Time.deltaTime;

            if (_kameraDisiSure >= kameraDisiTetiklemeSuresi)
                TriggerGameOver();
        }
        else
        {
            _kameraDisiSure = 0f;
        }
    }

    private bool IsPlayerOutsideCamera()
    {
        if (mainCamera == null || playerRenderer == null) return false;

        Vector3 screenPos = mainCamera.WorldToViewportPoint(playerRenderer.bounds.center);
        bool outsideX = screenPos.x < 0f || screenPos.x > 1f;
        bool outsideY = screenPos.y < 0f || screenPos.y > 1f;
        return outsideX || outsideY;
    }

    public void TriggerGameOver()
    {
        if (isDead) return;
        isDead = true;

        AudioListener.pause = true;

        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (videoPlayer != null) videoPlayer.Play();

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.interactable = true;
            uiCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeInUI());
        }
    }

    private IEnumerator FadeInUI()
    {
        float sayac = 0f;
        while (sayac < acilisSuresi)
        {
            sayac += Time.deltaTime;
            uiCanvasGroup.alpha = Mathf.Lerp(0f, 1f, sayac / acilisSuresi);
            yield return null;
        }
        uiCanvasGroup.alpha = 1f;
    }

    // --- BUTONLAR İÇİN YENİ EKLENEN FONKSİYONLAR ---

    // Bu fonksiyonu UI'daki "Yeniden Dene" butonunun OnClick() kısmına ekle
    public void Buton_YenidenDene()
    {
        StartCoroutine(FadeOutAndRestart());
    }

    // İstersen menüye dönmek için bir buton daha ekleyebilirsin
    public void Buton_AnaMenu()
    {
        StartCoroutine(FadeOutToMenu());
    }

    // -----------------------------------------------

    private void OnVideoEnd(VideoPlayer vp)
    {
        // Video bittiğinde otomatik yeniden başlasın istiyorsan bu kalabilir
        // İstemiyorsan bu satırı silebilirsin, sadece butonla başlar
        StartCoroutine(FadeOutAndRestart());
    }

    private IEnumerator FadeOutAndRestart()
    {
        if (siyahKapanisEkrani != null)
        {
            siyahKapanisEkrani.gameObject.SetActive(true);
            float sayac = 0f;

            while (sayac < kapanisSuresi)
            {
                sayac += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, sayac / kapanisSuresi);
                siyahKapanisEkrani.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        RestartCurrentScene();
    }

    private IEnumerator FadeOutToMenu()
    {
        if (siyahKapanisEkrani != null)
        {
            siyahKapanisEkrani.gameObject.SetActive(true);
            float sayac = 0f;

            while (sayac < kapanisSuresi)
            {
                sayac += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, sayac / kapanisSuresi);
                siyahKapanisEkrani.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // Ana menüye dönerken de kayıt bayrağını sıfırlıyoruz ki baştan başlasın
        PlayerPrefs.SetInt("LoadFromSave", 0);
        PlayerPrefs.Save();

        if (SceneFader.instance != null)
            SceneFader.instance.SahneDegistir("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    private void RestartCurrentScene()
    {
        // 0 yerine 1 yaparsan GameManager oyuncuyu kaydettiği son konuma ışınlar
        PlayerPrefs.SetInt("LoadFromSave", 1);
        PlayerPrefs.Save();

        if (SceneFader.instance != null)
        {
            SceneFader.instance.SahneDegistir(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}