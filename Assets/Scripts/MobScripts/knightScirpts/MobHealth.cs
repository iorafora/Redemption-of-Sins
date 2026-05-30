using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MobHealth : MonoBehaviour
{
    // In MobHealth.cs, add this line so other scripts can read the current health:
    public float CurrentHealth => currentHealth;
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    private float currentHealth;
    private float targetHealth;
    public float healthLerpSpeed = 5f;

    [Header("Can Barı Görseli")]
    public Image healthBarFill;

    [Header("Geri Tepme Ayarları")]
    public float knockbackForceX = 2f;
    public float knockbackForceY = 1.25f;

    [Header("Hit Efekti")]
    public float hitDuration = 0.3f;
    public float flashDuration = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isHit = false;

    private Transform canvasTransform;
    private BaseMob baseMob;

    // SceneTransitionZone bunu okur
    public bool IsDead => currentHealth <= 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseMob = GetComponent<BaseMob>();

        currentHealth = maxHealth;
        targetHealth = maxHealth;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;

            Canvas parentCanvas = healthBarFill.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                canvasTransform = parentCanvas.transform;
        }
    }

    void Update()
    {
        if (healthBarFill != null)
        {
            float targetFill = targetHealth / maxHealth;

            if (healthBarFill.fillAmount != targetFill)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, Time.deltaTime * healthLerpSpeed);

                if (Mathf.Abs(healthBarFill.fillAmount - targetFill) < 0.005f)
                    healthBarFill.fillAmount = targetFill;
            }
        }
    }

    void LateUpdate()
    {
        if (canvasTransform != null)
        {
            float bossSign = transform.localScale.x > 0 ? 1f : -1f;
            canvasTransform.localScale = new Vector3(bossSign * Mathf.Abs(canvasTransform.localScale.x), canvasTransform.localScale.y, 1f);
        }
    }

    public void TakeHit(Transform attacker, float damage)
    {
        if (IsDead) return;

        if (baseMob != null && baseMob.IsShielding && baseMob.IsAttackerInFront(attacker))
        {
            baseMob.OnHit(attacker);
            return;
        }

        currentHealth -= damage;
        targetHealth = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            targetHealth = 0;
            if (healthBarFill != null) healthBarFill.fillAmount = 0f;
            if (baseMob != null) baseMob.Die();
            return;
        }

        if (!isHit)
            StartCoroutine(HitRoutine(attacker));

        if (baseMob != null)
            baseMob.OnHit(attacker);
    }

    private IEnumerator HitRoutine(Transform attacker)
    {
        isHit = true;

        bool playerIsOnRight = attacker.position.x > transform.position.x;
        float knockDirX = playerIsOnRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockDirX * knockbackForceX, knockbackForceY);

        if (sr != null) sr.color = new Color(1f, 0.2f, 0.2f, 1f);
        yield return new WaitForSeconds(flashDuration);
        if (sr != null) sr.color = Color.white;

        yield return new WaitForSeconds(hitDuration - flashDuration);
        isHit = false;
    }
}