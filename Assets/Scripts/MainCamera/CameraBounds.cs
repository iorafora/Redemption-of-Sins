using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;
    public Rigidbody2D playerRb;
    public SpriteRenderer background; // Sahnenin background sprite'ını sürükle

    private Camera cam;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Background sprite'ının sınırlarını al
        if (background != null)
        {
            Bounds b = background.bounds;
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            minX = b.min.x + halfWidth;
            maxX = b.max.x - halfWidth;
            minY = b.min.y + halfHeight;
            maxY = b.max.y - halfHeight;
        }
    }

    void LateUpdate()
    {
        if (player == null || cam == null || background == null) return;

        // Kamerayı sınır içinde tut
        Vector3 camPos = cam.transform.position;
        float clampedCamX = Mathf.Clamp(camPos.x, minX, maxX);
        float clampedCamY = Mathf.Clamp(camPos.y, minY, maxY);
        cam.transform.position = new Vector3(clampedCamX, clampedCamY, camPos.z);

        // Playeri kameranın görünen alanı içinde tut
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 c = cam.transform.position;
        float pMinX = c.x - halfWidth + 0.5f;
        float pMaxX = c.x + halfWidth - 0.5f;
        float pMinY = c.y - halfHeight + 0.5f;
        float pMaxY = c.y + halfHeight - 0.5f;

        Vector3 pos = player.position;
        float cx = Mathf.Clamp(pos.x, pMinX, pMaxX);
        float cy = Mathf.Clamp(pos.y, pMinY, pMaxY);

        if (Mathf.Abs(cx - pos.x) > 0.001f || Mathf.Abs(cy - pos.y) > 0.001f)
        {
            player.position = new Vector3(cx, cy, pos.z);

            if (playerRb != null)
            {
                Vector2 vel = playerRb.linearVelocity;
                if (Mathf.Abs(cx - pos.x) > 0.001f) vel.x = 0f;
                if (Mathf.Abs(cy - pos.y) > 0.001f) vel.y = 0f;
                playerRb.linearVelocity = vel;
            }
        }
    }
}