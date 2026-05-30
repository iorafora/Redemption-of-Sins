using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("Diyalog Verisi")]
    public DialogueNode npcDialogue; // Bu NPC'nin baþlatacaðý diyalog dosyasý (Kaset)

    [Header("Görsel Ýpucu (E Tuþu)")]
    public GameObject interactPrompt; // NPC'nin baþýndaki 'E' harfi objesi

    private bool isPlayerInRange = false; // Oyuncu konuþma alanýnda mý?

    private void Start()
    {
        // Oyun baþlarken 'E' ipucunu otomatik olarak gizle
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Oyuncu alandaysa VE 'E' tuþuna bastýysa
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Eðer diyalog paneli þu an zaten açýk DEÐÝLSE yeni diyaloðu baþlat
            if (!DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                DialogueManager.Instance.StartDialogue(npcDialogue);

                // Konuþma baþlayýnca kafasýndaki 'E' harfini gizle (isteðe baðlý)
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(false);
                }
            }
        }
    }

    // Ana karakter trigger (tetikleyici) alanýna girdiðinde
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Alana giren objenin 'Player' (Oyuncu) etiketine sahip olup olmadýðýný kontrol et
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true); // 'E' harfini görünür yap
            }
        }
    }

    // Ana karakter alandan çýktýðýnda
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false); // 'E' harfini tekrar gizle
            }

            // Eðer oyuncu diyalog bitmeden arkasýný dönüp kaçarsa paneli kapat
            if (DialogueManager.Instance.dialoguePanel.activeSelf)
            {
                DialogueManager.Instance.dialoguePanel.SetActive(false);
            }
        }
    }
}