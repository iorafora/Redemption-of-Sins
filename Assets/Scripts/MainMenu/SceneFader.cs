using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    // Her sahnede sadece 1 tane olmasını sağlayan yapı
    public static SceneFader instance;

    [Header("Siyah Ekran Bileşeni")]
    public CanvasGroup fadeCanvasGroup;

    [Header("Geçiş Hızı (Saniye)")]
    public float gecisSuresi = 1.5f;

    private void Awake()
    {
        // Sahne değiştikçe bu sistemin yok olmasını engelliyoruz
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Sahne her yüklendiğinde otomatik olarak "Fade In" tetiklensin
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Yeni sahne açıldığında otomatik çalışan fonksiyon
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    // BUTONLARLA SAHNE DEĞİŞTİRMEK İÇİN BU ÇAĞRILACAK
    public void SahneDegistir(string sahneAdi)
    {
        StartCoroutine(FadeOut(sahneAdi));
    }

    // Ekranı Aç (Siyahtan Şeffafa)
    private IEnumerator FadeIn()
    {
        fadeCanvasGroup.blocksRaycasts = true; // Tıklamaları engelle
        float zaman = gecisSuresi;

        while (zaman > 0f)
        {
            zaman -= Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(zaman / gecisSuresi);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false; // Tıklamaları aç
    }

    // Ekranı Karart ve Sahneyi Yükle (Şeffaftan Siyaha)
    private IEnumerator FadeOut(string sahneAdi)
    {
        fadeCanvasGroup.blocksRaycasts = true; // Tıklamaları engelle
        float zaman = 0f;

        while (zaman < gecisSuresi)
        {
            zaman += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(zaman / gecisSuresi);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        // Sahneyi yükle ve bitmesini bekle
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sahneAdi);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}