using UnityEngine;

public class SpellHitbox : MonoBehaviour
{
    [Header("Büyü Ayarları")]
    public float spellDuration = 1.0f; // Büyünün ekranda kalma toplam süresi

    [Header("Hasar Ayarları")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1.5f); // Kırmızı kutunun büyüklüğü
    public Vector2 hitboxOffset = new Vector2(0f, 0f); // YENİ: Kırmızı kutuyu büyünün neresine kaydıracağın (X ve Y)
    public float damageDelay = 0.5f; // Büyü çıktıktan KAÇ SANİYE SONRA hasar vurmaya başlasın?

    private bool hasHit = false;
    private float currentDelayTimer;

    void OnEnable()
    {
        hasHit = false;
        currentDelayTimer = damageDelay;
        Invoke("DisableSpell", spellDuration);
    }

    void DisableSpell()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (hasHit) return;

        // Hasar gecikmesi süresi dolmadıysa bekle
        if (currentDelayTimer > 0)
        {
            currentDelayTimer -= Time.deltaTime;
            return;
        }

        // Hitbox'ın kaydırılmış asıl pozisyonunu hesapla
        Vector2 actualHitboxPosition = (Vector2)transform.position + hitboxOffset;

        // O pozisyona kutuyu çiz ve tarama yap
        Collider2D[] colliders = Physics2D.OverlapBoxAll(actualHitboxPosition, hitboxSize, 0f);

        foreach (Collider2D hit in colliders)
        {
            McCombat player = hit.GetComponent<McCombat>();

            if (player == null)
            {
                player = hit.GetComponentInParent<McCombat>();
            }

            if (player != null && !player.IsHit)
            {
                hasHit = true;
                player.TakeHit(transform);
                Debug.Log("Manuel Tarayıcı: Gecikmeli hasar başarıyla vuruldu!");
                break;
            }
        }
    }

    // Sahne ekranında kırmızı kutuyu (ve kaydırma miktarını) anlık olarak görebilmek için
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Kırmızı çizgiyi çizerken de offset değerini hesaba katıyoruz
        Vector2 actualHitboxPosition = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(actualHitboxPosition, hitboxSize);
    }
}