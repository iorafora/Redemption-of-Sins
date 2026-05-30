using UnityEngine;

public class BaseMob : MonoBehaviour, IMob
{
    [Header("Saldırı Hitbox")]
    public GameObject attackHitbox;

    // Tüm moblar için ortak "Ölü" bayrağı
    [HideInInspector] public bool isDead = false;

    protected virtual void Start()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    public void DealDamageToPlayer(McCombat playerCombat)
    {
        playerCombat.TakeHit(transform);
    }

    protected void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(true);
    }

    protected void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // --- YENİ EKLENEN ORTAK (VIRTUAL) FONKSİYONLAR ---
    // 'virtual' kelimesi, diğer mobların (Knight, Bringer) bu özellikleri kendi ihtiyaçlarına göre değiştirebilmesini (override) sağlar.

    public virtual bool IsShielding => false;

    public virtual bool IsAttackerInFront(Transform attacker)
    {
        return false;
    }

    public virtual void OnHit(Transform attacker)
    {
        // Standart vurulma tepkisi
    }

    public virtual void Die()
    {
        isDead = true;
        DisableHitbox();
    }
}