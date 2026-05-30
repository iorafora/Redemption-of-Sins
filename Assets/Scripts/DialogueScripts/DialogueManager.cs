using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Sistem Durumu")]
    public bool isDialogueActive = false; // Karakter hareketini dondurmak için

    [Header("UI Referanslarý")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Buton Referanslarý")]
    public GameObject choice1ButtonObj;
    public TextMeshProUGUI choice1Text;

    public GameObject choice2ButtonObj;
    public TextMeshProUGUI choice2Text;

    private DialogueNode currentNode;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueNode startNode)
    {
        isDialogueActive = true; // Diyalog baþladý, karakter dondu!
        dialoguePanel.SetActive(true);
        currentNode = startNode;
        RefreshUI();
    }

    private void RefreshUI()
    {
        nameText.text = currentNode.speakerName;
        dialogueText.text = currentNode.dialogueText;

        // 1. Seçenek
        if (!string.IsNullOrEmpty(currentNode.choice1Text))
        {
            choice1ButtonObj.SetActive(true);
            choice1Text.text = currentNode.choice1Text;
        }
        else
        {
            choice1ButtonObj.SetActive(false);
        }

        // 2. Seçenek
        if (!string.IsNullOrEmpty(currentNode.choice2Text))
        {
            choice2ButtonObj.SetActive(true);
            choice2Text.text = currentNode.choice2Text;
        }
        else
        {
            choice2ButtonObj.SetActive(false);
        }
    }

    public void SelectChoice1() { HandleChoice(currentNode.choice1NextNode); }
    public void SelectChoice2() { HandleChoice(currentNode.choice2NextNode); }

    private void HandleChoice(DialogueNode nextNode)
    {
        if (!string.IsNullOrEmpty(currentNode.eventCode))
        {
            Debug.Log("Oyunun kaderi deðiþti! Tetiklenen olay: " + currentNode.eventCode);
        }

        if (nextNode != null)
        {
            currentNode = nextNode;
            RefreshUI();
        }
        else
        {
            EndDialogue(); // Sýradaki dosya yoksa bitir
        }
    }

    // Diyaloðu tamamen kapatan fonksiyon
    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false; // Diyalog bitti, karakter özgür!
    }

    private void Update()
    {
        // Eðer panel aktif deðilse, aþaðýdaki tuþ kontrollerini hiç okuma
        if (!dialoguePanel.activeSelf) return;

        // ÝSTEK 1: 'Z' tuþu ile diyaloðu tamamen geç
        if (Input.GetKeyDown(KeyCode.Z))
        {
            EndDialogue();
            return;
        }

        // ÝSTEK 2: Sol týklama ile ilerle (Sadece seçenek butonlarý yoksa çalýþýr)
        if (!choice1ButtonObj.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) // 0 = Sol Týk
            {
                HandleChoice(currentNode.nextNode);
            }
        }
    }
}