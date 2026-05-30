using UnityEngine;
using UnityEngine.EventSystems; // Mouse hareketlerini algýlamak için

public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Süzülme (Idle) Ayarlarý")]
    public float floatSpeed = 2.5f;    // Süzülme hýzý
    public float floatAmount = 4f;     // Yukarý/aþaðý hareket mesafesi (piksel cinsinden)

    [Header("Üzerine Gelme (Hover) Ayarlarý")]
    public float hoverScaleMultiplier = 1.05f; // Mouse üzerindeyken %5 büyütür

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Manuel olarak verdiðin Pos X ve Pos Y deðerlerini kaydeder
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Eðer mouse üzerinde deðilse, pürüzsüz süzülme hareketini yap
        if (!isHovered)
        {
            float newY = originalPosition.y + (Mathf.Sin(Time.time * floatSpeed) * floatAmount);
            rectTransform.anchoredPosition = new Vector2(originalPosition.x, newY);
        }
        else
        {
            // Mouse üzerindeyken titremeyi önlemek için orijinal yerinde sabit tut
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // Mouse butonun üzerine geldiðinde
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        transform.localScale = originalScale * hoverScaleMultiplier; // Yumuþakça büyüt
    }

    // Mouse butonun üzerinden çekildiðinde
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        transform.localScale = originalScale; // Orijinal boyuta dön
    }
}