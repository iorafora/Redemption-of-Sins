using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class IntroSequence : MonoBehaviour
{
    [Header("Intro Görselleri")]
    public IntroFrame[] frames;

    [Header("Geçiş Ayarları")]
    public float fadeDuration = 0.5f;
    public float startDelay = 3f;

    [Header("Zoom Ayarları")]
    public float zoomAmount = 0.05f;
    public float zoomDuration = 4f;

    [Header("Nefes Animasyonu (Zoom Bitince)")]
    public float breathSpeed = 1.2f;
    public float breathSwayX = 5f;
    public float breathDepthScale = 0.012f;

    [Header("Ses Ayarları")]
    public AudioSource introMusic;
    public float maxMusicVolume = 1f;
    public AudioSource[] otherSounds;

    [Header("Referanslar")]
    public Image displayImage;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI storyTextComponent;
    public string nextSceneName = "FirstScene";

    private bool zPressed = false;
    private bool inputEnabled = false;
    private bool sequenceStarted = false;
    private Vector3 baseScale;
    private Coroutine zoomCoroutine;
    private Coroutine breathCoroutine;

    [System.Serializable]
    public class IntroFrame
    {
        public Sprite image;

        [TextArea(3, 5)]
        // Artık tek bir yazı değil, yazılar dizisi tutuyoruz:
        public string[] storyTexts;

        public float displayDuration = 3f;
    }

    void Start()
    {
        foreach (AudioSource s in otherSounds)
            if (s != null) s.mute = true;

        if (introMusic != null)
        {
            introMusic.volume = 0f;
            introMusic.Play();
        }

        baseScale = displayImage.transform.localScale;
        canvasGroup.alpha = 0f;

        StartCoroutine(PlaySequence());
        sequenceStarted = true;
    }

    void Update()
    {
        if (!sequenceStarted || !inputEnabled) return;
        if (Input.GetKeyDown(KeyCode.Z)) zPressed = true;
    }

    // ─────────────────────────────────────────────
    //  Ana Sekans
    // ─────────────────────────────────────────────
    private IEnumerator PlaySequence()
    {
        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(startDelay);
        inputEnabled = true;

        for (int i = 0; i < frames.Length; i++)
        {
            displayImage.sprite = frames[i].image;
            displayImage.transform.localScale = baseScale;
            zPressed = false;

            // Zoom süresini metin sayısına göre uzatıyoruz ki metinler okunurken zoom aniden durmasın
            float totalDuration = frames[i].displayDuration;
            if (frames[i].storyTexts != null && frames[i].storyTexts.Length > 0)
            {
                totalDuration = frames[i].storyTexts.Length * frames[i].displayDuration;
            }

            // Zoom başlat
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomRoutine(totalDuration + fadeDuration * 2f));

            // Fade In
            yield return StartCoroutine(Fade(0f, 1f));

            bool playerSkipped = false;

            // ── Paragrafları Sırayla Gösterme Döngüsü ──────────────
            if (frames[i].storyTexts != null && frames[i].storyTexts.Length > 0)
            {
                for (int t = 0; t < frames[i].storyTexts.Length; t++)
                {
                    if (storyTextComponent != null)
                    {
                        storyTextComponent.text = frames[i].storyTexts[t];
                    }

                    float elapsed = 0f;
                    // Ya süre dolana kadar bekle ya da Z'ye basılana kadar
                    while (elapsed < frames[i].displayDuration && !zPressed)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (zPressed)
                    {
                        zPressed = false;
                        // Eğer bu RESMİN SON PARAGRAFINDA Z'ye basıldıysa, resmi geç (nefesi atla)
                        if (t == frames[i].storyTexts.Length - 1)
                        {
                            playerSkipped = true;
                        }
                    }
                }
            }
            else
            {
                // Eğer o frame için hiç yazı girilmemişse sadece resmi bekle
                if (storyTextComponent != null) storyTextComponent.text = "";

                float elapsed = 0f;
                while (elapsed < frames[i].displayDuration && !zPressed)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                playerSkipped = zPressed;
                zPressed = false;
            }
            // ───────────────────────────────────────────────────────

            // Zoom durdur
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);

            // ── Zoom bitti, Z basılmadıysa → Nefes ──────────────
            if (!playerSkipped)
            {
                Vector3 zoomedScale = displayImage.transform.localScale;

                if (breathCoroutine != null) StopCoroutine(breathCoroutine);
                breathCoroutine = StartCoroutine(BreathRoutine(zoomedScale));

                // Diğer resme geçmek için Z bekle
                while (!zPressed)
                    yield return null;

                zPressed = false;

                if (breathCoroutine != null) StopCoroutine(breathCoroutine);
            }

            // Fade Out olurken metni temizle ki görsel kaybolurken metin havada kalmasın
            if (storyTextComponent != null) storyTextComponent.text = "";
            yield return StartCoroutine(Fade(1f, 0f));
            displayImage.transform.localScale = baseScale;
        }

        foreach (AudioSource s in otherSounds)
            if (s != null) s.mute = false;

        if (introMusic != null) introMusic.Stop();

        if (SceneFader.instance != null)
            SceneFader.instance.SahneDegistir(nextSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator ZoomRoutine(float totalDuration)
    {
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / zoomDuration);
            float zoom = baseScale.x * (1f + zoomAmount * progress);
            displayImage.transform.localScale = new Vector3(zoom, zoom, 1f);
            yield return null;
        }
    }

    private IEnumerator BreathRoutine(Vector3 fromScale)
    {
        float time = 0f;
        Vector3 pos = displayImage.transform.localPosition;
        while (true)
        {
            time += Time.deltaTime;
            float swayX = Mathf.Sin(time * breathSpeed) * breathSwayX;
            float breathMult = 1f + Mathf.Sin(time * breathSpeed * 0.75f) * breathDepthScale;

            displayImage.transform.localPosition = new Vector3(pos.x + swayX, pos.y, pos.z);
            displayImage.transform.localScale = new Vector3(
                fromScale.x * breathMult,
                fromScale.y * breathMult,
                fromScale.z
            );
            yield return null;
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            if (introMusic != null)
                introMusic.volume = Mathf.Lerp(from * maxMusicVolume, to * maxMusicVolume, t);
            yield return null;
        }
        canvasGroup.alpha = to;
        if (introMusic != null)
            introMusic.volume = to * maxMusicVolume;
    }
}