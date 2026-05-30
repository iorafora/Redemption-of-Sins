using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Sahneler arası geçiş için bu kütüphane eklendi

public class SceneIntroManager : MonoBehaviour
{
    [Header("--- SAHNE GİRİŞ (İLK AÇILIŞ) AYARLARI ---")]
    public CanvasGroup introCanvasGroup;
    public float introBeklemeSuresi = 5f;
    public float introFadeOutSuresi = 1.5f;

    [Header("--- BOSS ÖLÜM (FİREGUY) AYARLARI ---")]
    public MobHealth targetBoss;

    public CanvasGroup siyahEkranCanvasGroup;
    public CanvasGroup bossDeathCanvasGroup;

    public float siyahFadeInSuresi = 1f;
    public float resimFadeInSuresi = 1.5f;
    public float bossBeklemeSuresi = 4f;
    public float tumFadeOutSuresi = 1.5f;

    private bool isBossTriggered = false;
    private bool bossSahnedeVarMi = false; // Sistemin boss'u takip etmesi için yeni hafıza

    void Start()
    {
        // 1. Sahne açılış görselini hazırla
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 1f;
            introCanvasGroup.gameObject.SetActive(true);
            StartCoroutine(IntroSekansi());
        }

        // 2. Siyah ekranı ve boss resmini gizli tut
        if (siyahEkranCanvasGroup != null)
        {
            siyahEkranCanvasGroup.alpha = 0f;
            siyahEkranCanvasGroup.gameObject.SetActive(false);
        }

        if (bossDeathCanvasGroup != null)
        {
            bossDeathCanvasGroup.alpha = 0f;
            bossDeathCanvasGroup.gameObject.SetActive(false);
        }

        // 3. Eğer Inspector'dan boss atandıysa, "Evet bu sahnede bir boss var" diye hafızaya al
        if (targetBoss != null)
        {
            bossSahnedeVarMi = true;
        }
    }

    void Update()
    {
        // Eğer bu sahnede bir boss varsa ve sinematik henüz tetiklenmediyse sürekli kontrol et
        if (bossSahnedeVarMi && !isBossTriggered)
        {
            // Boss'un canı 0 olduysa (IsDead) VEYA Boss oyundan tamamen silindiyse (null)
            if (targetBoss == null || targetBoss.IsDead)
            {
                isBossTriggered = true;
                StartCoroutine(BossDeathSekansi());
            }
        }
    }

    // --- SAHNE GİRİŞ KORUTİNİ ---
    private IEnumerator IntroSekansi()
    {
        LockPlayer(true);
        yield return new WaitForSeconds(introBeklemeSuresi);

        float sayac = 0;
        while (sayac < introFadeOutSuresi)
        {
            sayac += Time.deltaTime;
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, sayac / introFadeOutSuresi);
            yield return null;
        }

        introCanvasGroup.gameObject.SetActive(false);
        LockPlayer(false);
    }

    // --- BOSS ÖLÜM SİNEMATİĞİ ---
    private IEnumerator BossDeathSekansi()
    {
        LockPlayer(true);

        if (siyahEkranCanvasGroup != null) siyahEkranCanvasGroup.gameObject.SetActive(true);
        if (bossDeathCanvasGroup != null) bossDeathCanvasGroup.gameObject.SetActive(true);

        float sayac = 0;
        while (sayac < siyahFadeInSuresi)
        {
            sayac += Time.deltaTime;
            if (siyahEkranCanvasGroup != null)
                siyahEkranCanvasGroup.alpha = Mathf.Lerp(0f, 1f, sayac / siyahFadeInSuresi);
            yield return null;
        }

        sayac = 0;
        while (sayac < resimFadeInSuresi)
        {
            sayac += Time.deltaTime;
            if (bossDeathCanvasGroup != null)
                bossDeathCanvasGroup.alpha = Mathf.Lerp(0f, 1f, sayac / resimFadeInSuresi);
            yield return null;
        }

        yield return new WaitForSeconds(bossBeklemeSuresi);

        sayac = 0;
        while (sayac < tumFadeOutSuresi)
        {
            sayac += Time.deltaTime;
            float azalma = Mathf.Lerp(1f, 0f, sayac / tumFadeOutSuresi);

            if (siyahEkranCanvasGroup != null) siyahEkranCanvasGroup.alpha = azalma;
            if (bossDeathCanvasGroup != null) bossDeathCanvasGroup.alpha = azalma;

            yield return null;
        }

        if (siyahEkranCanvasGroup != null) siyahEkranCanvasGroup.gameObject.SetActive(false);
        if (bossDeathCanvasGroup != null) bossDeathCanvasGroup.gameObject.SetActive(false);

        LockPlayer(false);

        // --- YENİ EKLENEN KISIM: Animasyon bittiğinde Ana Menüye dön ---
        SceneManager.LoadScene("MainMenu");
    }

    private void LockPlayer(bool lockState)
    {
        McMovement playerMove = FindAnyObjectByType<McMovement>();
        McCombat playerCombat = FindAnyObjectByType<McCombat>();

        if (playerMove != null) playerMove.enabled = !lockState;
        if (playerCombat != null) playerCombat.enabled = !lockState;
    }
}