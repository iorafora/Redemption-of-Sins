using UnityEngine;

[RequireComponent(typeof(Camera))]
public class McCamera : MonoBehaviour
{
    [Header("Kamera Duvarı Ayarları")]
    public bool altTarafiAcikBirak = true;

    [Header("Ölüm Yakınlaştırması (DeathZoom)")]
    public float deathZoomTargetSize = 2.5f;
    public float deathZoomSpeed = 1.5f;

    [Header("Takip Ayarları")]
    public Transform player;
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Mob Ortalama Ayarları")]
    public Transform mobsContainer;
    public float mobDetectDistance = 8f;
    public float returnDistance = 12f;

    [Header("X Ekseni Canlılık (Sağa - Sola)")]
    public bool enableSwayX = true;
    public float swayAmountX = 0.1f;
    public float swaySpeedX = 0.3f;

    [Header("Y Ekseni Canlılık (Aşağı - Yukarı)")]
    public bool enableSwayY = true;
    public float swayAmountY = 0.1f;
    public float swaySpeedY = 0.3f;

    [Header("Z Ekseni Canlılık (Zoom / Nefes Alma)")]
    public bool enableSwayZ = true;
    public float swayAmountZ = 0.1f;
    public float swaySpeedZ = 0.2f;

    [Header("Dinamik Non-Convex Sınır (Polygon)")]
    public PolygonCollider2D backgroundBoundary;

    private Vector3 currentVelocity = Vector3.zero;
    private Camera cam;
    private EdgeCollider2D edgeBounds;
    private McCombat playerCombat;

    private float randomSeedX;
    private float randomSeedY;
    private float randomSeedZ;
    private float baseOrthoSize;
    private float dynamicZMultiplier = 1f;

    private bool isDeathZooming = false;

    // UYARI DÜZELTİLDİ: int yerine Collider2D tipinde liste tutuyoruz
    private readonly System.Collections.Generic.HashSet<Collider2D> _ignoredMobs =
        new System.Collections.Generic.HashSet<Collider2D>();

    private float _ignoreRefreshTimer = 0f;
    private const float IGNORE_REFRESH_INTERVAL = 1f;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthoSize = cam.orthographicSize;

        isDeathZooming = false;

        if (player != null)
        {
            playerCombat = player.GetComponent<McCombat>();
        }

        randomSeedX = Random.Range(0f, 100f);
        randomSeedY = Random.Range(0f, 100f);
        randomSeedZ = Random.Range(0f, 100f);

        if (backgroundBoundary == null)
        {
            Debug.LogError("McCamera: PolygonCollider2D atanmamış!");
        }

        SetupCameraBoundsCollider();
    }

    public void TriggerDeathZoom()
    {
        isDeathZooming = true;
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (isDeathZooming)
        {
            Vector3 targetPos = new Vector3(player.position.x, player.position.y, offset.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * deathZoomSpeed);

            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, deathZoomTargetSize, Time.deltaTime * deathZoomSpeed);
            }
            return;
        }

        float targetX = player.position.x;
        float targetY = player.position.y;
        float targetZ = offset.z;

        bool isInCombat = false;
        Transform nearestMob = GetNearestMob();

        if (nearestMob != null)
        {
            float distToMob = Vector2.Distance(player.position, nearestMob.position);

            if (distToMob <= mobDetectDistance)
            {
                targetX = (player.position.x + nearestMob.position.x) / 2f;
                targetY = (player.position.y + nearestMob.position.y) / 2f;
                isInCombat = true;
            }
            else if (distToMob < returnDistance)
            {
                float t = (distToMob - mobDetectDistance) / (returnDistance - mobDetectDistance);
                float midX = (player.position.x + nearestMob.position.x) / 2f;
                targetX = Mathf.Lerp(midX, player.position.x, t);
                float midY = (player.position.y + nearestMob.position.y) / 2f;
                targetY = Mathf.Lerp(midY, player.position.y, t);
                isInCombat = true;
            }
        }

        if (playerCombat != null && (playerCombat.IsAttacking || playerCombat.IsHit))
        {
            isInCombat = true;
        }

        if (enableSwayX) targetX += (Mathf.PerlinNoise(Time.time * swaySpeedX + randomSeedX, 0f) - 0.5f) * swayAmountX;
        if (enableSwayY) targetY += (Mathf.PerlinNoise(0f, Time.time * swaySpeedY + randomSeedY) - 0.5f) * swayAmountY;

        float targetMultiplier = isInCombat ? 0f : 1f;
        dynamicZMultiplier = Mathf.Lerp(dynamicZMultiplier, targetMultiplier, Time.deltaTime * 4f);

        if (enableSwayZ)
        {
            float swayZValue = (Mathf.PerlinNoise(Time.time * swaySpeedZ, randomSeedZ) - 0.5f) * swayAmountZ * dynamicZMultiplier;

            if (cam.orthographic) cam.orthographicSize = baseOrthoSize + swayZValue;
            else targetZ += swayZValue;
        }

        Vector3 finalTargetPosition = new Vector3(targetX + offset.x, targetY + offset.y, targetZ);

        if (backgroundBoundary != null)
        {
            finalTargetPosition = AdjustPositionToPolygon(finalTargetPosition);
        }

        transform.position = Vector3.SmoothDamp(transform.position, finalTargetPosition, ref currentVelocity, smoothTime);
        UpdateCameraBoundsCollider();

        _ignoreRefreshTimer -= Time.deltaTime;
        if (_ignoreRefreshTimer <= 0f)
        {
            _ignoreRefreshTimer = IGNORE_REFRESH_INTERVAL;
            RefreshMobIgnoreList();
        }
    }

    private void SetupCameraBoundsCollider()
    {
        edgeBounds = GetComponent<EdgeCollider2D>();
        if (edgeBounds == null) edgeBounds = gameObject.AddComponent<EdgeCollider2D>();

        edgeBounds.isTrigger = false;
        edgeBounds.edgeRadius = 0.5f;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        RefreshMobIgnoreList();
    }

    private void RefreshMobIgnoreList()
    {
        if (edgeBounds == null || mobsContainer == null) return;

        Collider2D[] allMobCols = mobsContainer.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in allMobCols)
        {
            // UYARI DÜZELTİLDİ: GetInstanceID() yerine direkt objenin kendisini kullanıyoruz
            if (_ignoredMobs.Contains(col)) continue;

            Physics2D.IgnoreCollision(edgeBounds, col, true);
            _ignoredMobs.Add(col);
        }
    }

    private void UpdateCameraBoundsCollider()
    {
        if (edgeBounds == null || !cam.orthographic) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        if (altTarafiAcikBirak)
        {
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(-halfWidth, -halfHeight);
            points[1] = new Vector2(-halfWidth, halfHeight);
            points[2] = new Vector2(halfWidth, halfHeight);
            points[3] = new Vector2(halfWidth, -halfHeight);
            edgeBounds.points = points;
        }
        else
        {
            Vector2[] points = new Vector2[5];
            points[0] = new Vector2(-halfWidth, -halfHeight);
            points[1] = new Vector2(-halfWidth, halfHeight);
            points[2] = new Vector2(halfWidth, halfHeight);
            points[3] = new Vector2(halfWidth, -halfHeight);
            points[4] = new Vector2(-halfWidth, -halfHeight);
            edgeBounds.points = points;
        }
    }

    private Vector3 AdjustPositionToPolygon(Vector3 targetCenter)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        for (int i = 0; i < 4; i++)
        {
            Vector2 currentCenter = (Vector2)targetCenter;

            Vector2[] corners = new Vector2[4];
            corners[0] = currentCenter + new Vector2(-halfWidth, -halfHeight);
            corners[1] = currentCenter + new Vector2(halfWidth, -halfHeight);
            corners[2] = currentCenter + new Vector2(-halfWidth, halfHeight);
            corners[3] = currentCenter + new Vector2(halfWidth, halfHeight);

            Vector2 worstOffendingAdjustment = Vector2.zero;
            float maxAdjustmentDistance = -1f;

            for (int c = 0; c < 4; c++)
            {
                if (!backgroundBoundary.OverlapPoint(corners[c]))
                {
                    Vector2 closestValidPoint = backgroundBoundary.ClosestPoint(corners[c]);
                    Vector2 adjustmentVector = closestValidPoint - corners[c];
                    float dist = adjustmentVector.sqrMagnitude;

                    if (dist > maxAdjustmentDistance)
                    {
                        maxAdjustmentDistance = dist;
                        worstOffendingAdjustment = adjustmentVector;
                    }
                }
            }

            if (worstOffendingAdjustment != Vector2.zero) targetCenter += (Vector3)worstOffendingAdjustment;
            else break;
        }
        return targetCenter;
    }

    private Transform GetNearestMob()
    {
        if (mobsContainer == null) return null;
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (Transform mob in mobsContainer)
        {
            if (!mob.gameObject.activeInHierarchy) continue;
            float dist = Vector2.Distance(player.position, mob.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mob;
            }
        }
        return nearest;
    }

    private void OnDrawGizmos()
    {
        if (backgroundBoundary != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < backgroundBoundary.pathCount; i++)
            {
                Vector2[] path = backgroundBoundary.GetPath(i);
                for (int j = 0; j < path.Length; j++)
                {
                    Vector3 p1 = backgroundBoundary.transform.TransformPoint(path[j]);
                    Vector3 p2 = backgroundBoundary.transform.TransformPoint(path[(j + 1) % path.Length]);
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }
}