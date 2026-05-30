using UnityEngine;

public class SceneTransitionZone : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    public float bossCheckRadius = 200f;

    [Header("Referanslar")]
    public Transform player;
    public Transform mobsContainer;

    private bool transitioning = false;

    // Sahne sırası — build settings ile aynı
    private string[] sceneOrder = new string[]
    {
        "MainMenu",
        "FirstScene",
        "SecondScene,",
        "ThirdScene",
        "FourthScene",
        "FifthScene",
        "SixthScene",
        "SeventhScene",
        "FinalScene"
    };

    void Update()
    {
        if (player == null || transitioning) return;

        bool atEdge = IsPlayerAtRightEdge();
        bool cleared = CheckBossesCleared();
        string next = GetNextScene();

     

        if (!atEdge) return;
        if (!cleared) return;
        if (string.IsNullOrEmpty(next)) return;

        // SceneFader null kontrolü
        if (SceneFader.instance == null)
        {
            Debug.LogError("SceneFader bulunamadı! DontDestroyOnLoad objesini kontrol et.");
            return;
        }

        transitioning = true;
        SceneFader.instance.SahneDegistir(next);
    }

    private string GetNextScene()
    {
        string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        for (int i = 0; i < sceneOrder.Length - 1; i++)
        {
            if (sceneOrder[i] == current)
                return sceneOrder[i + 1];
        }

        return ""; // Son sahneyse geçiş yok
    }

    private bool CheckBossesCleared()
    {
        if (mobsContainer == null) return true;

        foreach (Transform mob in mobsContainer)
        {
            if (!mob.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(player.position, mob.position);
            if (dist > bossCheckRadius) continue;

            MobHealth health = mob.GetComponent<MobHealth>();
            if (health != null && !health.IsDead)
                return false;
        }

        return true;
    }

    private bool IsPlayerAtRightEdge()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float rightEdge = cam.transform.position.x + halfWidth;
        return player.position.x >= rightEdge - 1f;
    }
}