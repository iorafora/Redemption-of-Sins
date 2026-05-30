using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer sr;
    private float alpha;
    private float fadeSpeed;

    public void Init(Sprite sprite, Vector3 position, Vector3 scale, float startAlpha, float fade)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 1f, 1f, startAlpha); // Beyaz
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;
        transform.position = position;
        transform.localScale = scale;
        alpha = startAlpha;
        fadeSpeed = fade;
    }

    void Update()
    {
        alpha -= fadeSpeed * Time.deltaTime;
        sr.color = new Color(1f, 1f, 1f, alpha); // Beyaz kalır, sadece alpha düşer
        if (alpha <= 0f)
            Destroy(gameObject);
    }
}