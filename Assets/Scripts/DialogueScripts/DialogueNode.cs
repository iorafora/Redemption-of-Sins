using UnityEngine;

// Bu satýr, Unity editöründe sað týklayýp yeni bir diyalog oluþturmaný saðlar.
[CreateAssetMenu(fileName = "YeniDiyalog", menuName = "Diyalog Sistemi/Diyalog Nodu")]
public class DialogueNode : ScriptableObject
{
    [Header("Diyalog Bilgileri")]
    public string speakerName; // Konuþan kiþinin adý

    [TextArea(3, 10)] // Metin kutusunu editörde geniþ gösterir
    public string dialogueText; // Karakterin söyleyeceði söz

    [Header("1. Seçenek (Varsa)")]
    public string choice1Text; // Butonda yazacak yazý
    public DialogueNode choice1NextNode; // Bu seçilirse hangi diyaloga geçilecek?

    [Header("2. Seçenek (Varsa)")]
    public string choice2Text; // Butonda yazacak yazý
    public DialogueNode choice2NextNode; // Bu seçilirse hangi diyaloga geçilecek?

    [Header("Oyunun Kaderini Etkileyen Olay")]
    public string eventCode; // Örn: "KralinMektubuAlindi" veya "SavasBasladi"

    [Header("Eðer Seçenek Yoksa Gelecek Diyalog")]
    public DialogueNode nextNode; // Düz hikaye akýþý için
}