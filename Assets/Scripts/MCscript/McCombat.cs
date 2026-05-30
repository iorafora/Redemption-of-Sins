using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class McCombat : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float hitDamage = 10f;
    public float currentDamage = 0f;

    [Header("Saldırı Ayarları")]
    public float attackAnimDuration = 0.25f;
    public float attackHitDelay = 0.05f;
    public float attackRange = 1.2f;

    [Header("Hasar & Geri Tepme Ayarları")]
    public float knockbackForceX = 4f;
    public float knockbackForceY = 2.5f;
    public float hitDuration = 0.4f;
    public float iFrameDuration = 0.8f;
    public float movementLockDuration = 0.1f;

    [Header("Hit Renk Efekti")]
    public float flashDuration = 0.15f;

    [Header("Mob Layer")]
    public LayerMask mobLayer;

    [Header("Ses Efektleri (SFX)")]
    public AudioSource audioSource;
    public AudioClip attackSound1;
    [Range(0f, 1f)] public float attackSound1Volume = 1f;
    public AudioClip attackSound2;
    [Range(0f, 1f)] public float attackSound2Volume = 1f;
    public AudioClip damageSound;
    [Range(0f, 1f)] public float damageSoundVolume = 1f;

    [Header("Ekran Solma & Kenar Efekti (Post-Processing)")]
    public Volume globalVolume;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    public float maxVignetteIntensity = 0.6f;

    private float attackTimer = 0f;
    private bool isHit = false;
    private bool isAttacking = false;
    private bool isInvincible = false;

    public bool isDead { get; private set; } = false;

    public bool IsHit => isHit;
    public bool IsAttacking => isAttacking;
    public bool IsMovementLocked { get; private set; } = false;
    public bool AttackDirLeft { get; private set; }
    private Coroutine attackCoroutine;

    private Animator anim;
    private Rigidbody2D rb;
    private McMovement movement;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<McMovement>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
        }

        UpdateScreenColorFading();
    }

    void Update()
    {
        if (anim == null || rb == null || movement == null) return;

        if (isDead) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (isHit || isAttacking) return;

        if (Input.GetKeyDown(KeyCode.V) && attackTimer <= 0)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        attackTimer = attackAnimDuration;
        AttackDirLeft = movement.lastDirLeft;

        if (AttackDirLeft) anim.Play("hitting_left");
        else anim.Play("hitting_right");

        yield return new WaitForSeconds(attackHitDelay);

        Vector2 hitDirection = AttackDirLeft ? Vector2.left : Vector2.right;
        Vector2 hitCenter = (Vector2)transform.position + hitDirection * (attackRange * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackRange * 0.5f, mobLayer);
        bool successfullyHitAnyMob = false;

        foreach (Collider2D hit in hits)
        {
            MobHealth mob = hit.GetComponent<MobHealth>();
            BaseMob baseMob = hit.GetComponent<BaseMob>();

            if (mob != null)
            {
                bool isShielding = (baseMob != null) && baseMob.IsShielding;
                mob.TakeHit(transform, 10f);
                if (!isShielding) successfullyHitAnyMob = true;
            }
        }

        if (successfullyHitAnyMob && audioSource != null)
        {
            bool chooseFirst = Random.value > 0.5f;
            AudioClip clipToPlay = chooseFirst ? attackSound1 : attackSound2;
            float volumeToUse = chooseFirst ? attackSound1Volume : attackSound2Volume;
            if (clipToPlay != null) audioSource.PlayOneShot(clipToPlay, volumeToUse);
        }

        float remainingTime = attackAnimDuration - attackHitDelay;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    public void CancelAttack()
    {
        if (isAttacking)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            isAttacking = false;
            attackTimer = 0f;
        }
    }

    public void TakeHit(Transform attacker)
    {
        if (isInvincible || movement.isDashing || isDead) return;

        CancelAttack();
        currentHealth = Mathf.Max(0f, currentHealth - hitDamage);
        currentDamage = maxHealth - currentHealth;

        UpdateScreenColorFading();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitRoutine(attacker));
        }
    }

    private void Die()
    {
        isDead = true;
        isAttacking = false;
        isInvincible = true;
        IsMovementLocked = true;

        if (anim != null) anim.speed = 0f;
        if (sr != null) sr.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = 3f;
        }

        // UYARI DÜZELTİLDİ: FindFirstObjectByType yerine FindAnyObjectByType kullanıldı
        McCamera cam = Object.FindAnyObjectByType<McCamera>();
        if (cam != null) cam.TriggerDeathZoom();

        StartCoroutine(WaitAndTriggerGameOver());
    }

    private IEnumerator WaitAndTriggerGameOver()
    {
        yield return new WaitForSeconds(3f);

        // UYARI DÜZELTİLDİ: FindFirstObjectByType yerine FindAnyObjectByType kullanıldı
        GameOverManager gom = Object.FindAnyObjectByType<GameOverManager>();
        if (gom != null)
        {
            gom.TriggerGameOver();
        }
        else
        {
            Debug.LogError("Sahne içinde GameOverManager bulunamadı!");
        }
    }

    private void UpdateScreenColorFading()
    {
        float healthRatio = currentHealth / maxHealth;

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(maxVignetteIntensity, 0f, healthRatio);

        if (colorAdjustments != null)
            colorAdjustments.saturation.value = Mathf.Lerp(-60f, 0f, healthRatio);
    }

    private IEnumerator HitRoutine(Transform attacker)
    {
        isHit = true;
        isInvincible = true;
        isAttacking = false;
        IsMovementLocked = true;

        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound, damageSoundVolume);

        bool mobIsOnRight = attacker.position.x > transform.position.x;

        if (mobIsOnRight) anim.Play("hit_animation_right");
        else anim.Play("hit_animation_left");

        float knockDirX = mobIsOnRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockDirX * knockbackForceX, knockbackForceY);

        if (sr != null) sr.color = new Color(1f, 0.2f, 0.2f, 1f);
        yield return new WaitForSeconds(flashDuration);
        if (sr != null) sr.color = Color.white;

        yield return new WaitForSeconds(movementLockDuration - flashDuration);
        IsMovementLocked = false;

        yield return new WaitForSeconds(hitDuration - movementLockDuration);
        isHit = false;

        float remainingIFrame = iFrameDuration - hitDuration;
        if (remainingIFrame > 0) yield return new WaitForSeconds(remainingIFrame);

        isInvincible = false;
    }
}