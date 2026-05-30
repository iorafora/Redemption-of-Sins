using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutine kullanabilmek için gerekli

public class ImageToMainMenu : MonoBehaviour
{
    [Header("Geçiþ Ayarlarý")]
    [Tooltip("Görselin ekranda kalma ve kaybolma süresinin toplamý (saniye)")]
    public float beklemeSuresi = 5f;
    public string mainMenuSceneName = "MainMenu";

    // OnEnable fonksiyonu, bu scriptin baðlý olduðu obje (FireguyDeathImage) 
    // SetActive(true) ile görünür yapýldýðý an otomatik olarak çalýþýr.
    void OnEnable()
    {
        // Zamanlayýcýyý baþlat
        StartCoroutine(WaitAndLoadMenu());
    }

    IEnumerator WaitAndLoadMenu()
    {
        // Yazdýðýmýz süre kadar arka planda sessizce bekle
        yield return new WaitForSeconds(beklemeSuresi);

        // Süre dolduðunda Ana Menü sahnesini yükle
        Debug.Log("Kapanýþ görseli bitti, Ana Menüye dönülüyor...");

        // Eðer önceki adýmlarda kurduðun SceneFader varsa onu kullanabilirsin:
        // SceneFader.instance.SahneDegistir(mainMenuSceneName);

        // Yoksa standart yükleme:
        SceneManager.LoadScene(mainMenuSceneName);
    }
}