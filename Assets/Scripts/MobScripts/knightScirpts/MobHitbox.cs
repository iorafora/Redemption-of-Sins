using UnityEngine;

public class MobHitbox : MonoBehaviour
{
    private IMob parentMob;

    void Awake()
    {
        parentMob = GetComponentInParent<IMob>();

        if (parentMob == null)
            Debug.LogError($"MobHitbox: {transform.parent.name} üzerinde IMob bulunamadı!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentMob == null) return;

        // 1. Önce çarptığı objenin kendisinde McCombat var mı diye bakar:
        McCombat playerCombat = other.GetComponent<McCombat>();

        // 2. Eğer bulamazsa, belki çarptığım şey oyuncunun alt objesidir diyerek Ana Objeye (Parent) bakar:
        if (playerCombat == null)
        {
            playerCombat = other.GetComponentInParent<McCombat>();
        }

        // Eğer McCombat'ı bulabildiyse, hasarı çakar!
        if (playerCombat != null)
        {
            parentMob.DealDamageToPlayer(playerCombat);
        }
    }
}