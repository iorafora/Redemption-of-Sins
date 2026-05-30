using UnityEngine;

/// <summary>
/// Her merdiven GameObject'ine ekle.
/// BoxCollider2D otomatik eklenir ve trigger olarak ayarlanır.
/// 
/// KURULUM:
///   1. Bu scripti merdiven GameObject'ine ekle
///   2. BoxCollider2D boyutunu merdiveni kaplayacak şekilde ayarla (Is Trigger otomatik)
///   3. Oyuncunun Tag'ı "Player" olmalı
///   4. McMovement.cs'in güncel halini kullan (AddLadder / RemoveLadder metodları ekli)
///
/// Birden fazla merdiven sorunsuz çalışır:
///   Karakter bir merdivene girince _ladderCount artar, çıkınca azalır.
///   İki merdiven üst üste olsa bile doğru çalışır.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class LadderZone : MonoBehaviour
{
    private void Awake()
    {
        // Collider'ı her zaman trigger yap (elle işaretlemene gerek yok)
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        McMovement mc = other.GetComponent<McMovement>();
        if (mc != null) mc.AddLadder();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        McMovement mc = other.GetComponent<McMovement>();
        if (mc != null) mc.RemoveLadder();
    }
}
