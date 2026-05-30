using UnityEngine;
using System.Collections;

public class BringerOfDeath : BaseMob
{
    [Header("Ölüm Ayarları")]
    public float deathAnimDuration = 2.0f;

    [Header("Boyut Ayarı")]
    public float targetScale = 1f;

    [Header("Ses Ayarları (Audio)")]
    public AudioClip spellThunderSound;
    [Range(0f, 1f)] public float spellThunderVolume = 1f;

    public AudioClip meleeSlashSound;
    [Range(0f, 1f)] public float meleeSlashVolume = 1f;

    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1f;

    private AudioSource audioSource;

    [Header("Mesafe Ayarları")]
    public Transform player;
    public float visionDistance = 12f;
    public float chaseDistance = 10f;
    public float spellDistance = 7f;
    public float attackDistance = 2f;
    public float runSpeed = 3.5f;

    [Header("Bekleme (Cooldown) Ayarları")]
    public float meleeCooldown = 2f;
    public float spellCooldownMin = 1f;
    public float spellCooldownMax = 2f;

    [Header("Hasar Ayarları")]
    public float meleeDamage = 15f;
    public float spellDamage = 10f;

    [Header("Animasyon Süreleri")]
    public float attackAnimDuration = 1.0f;

    public float baseHitStunDuration = 0.4f;

    [Header("Stun Direnci (Anti-Spam) Ayarları")]
    public float hitStunDecay = 0.15f;
    public float minHitStun = 0f;
    public float stunResetTime = 2.0f;

    [Header("Vuruş Zamanlaması (Yakın Dövüş)")]
    public float hitTime = 0.5f;

    [Header("Bağımsız Büyü (Spell) Ayarları")]
    public GameObject spellEffectObject;
    public float castAnimDuration = 1.0f;
    public float spellOffsetY = 0f;
    public float doubleSpellInterval = 0.5f; // İki büyü arasındaki süre

    private float meleeTimer;
    private float spellTimer;
    private float currentStunDuration;
    private float lastHitTime = -10f;

    private bool isAttackLocked = false;
    private float attackLockTimer = 0f;
    private bool isStunned = false;
    private bool isCasting = false;

    private float attackStartTime = -1f;
    private bool hasHitDone = false;

    private Rigidbody2D rb;
    private Animator anim;
    private MobHealth mobHealth;

    private enum State { Rest, Idle, Run, MeleeAttack, SpellAttack }
    private State currentState;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        mobHealth = GetComponent<MobHealth>();
        currentState = State.Rest;

        currentStunDuration = baseHitStunDuration;

        audioSource = gameObject.AddComponent<AudioSource>();

        if (spellEffectObject != null)
            spellEffectObject.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        if (Time.time - lastHitTime > stunResetTime)
        {
            currentStunDuration = baseHitStunDuration;
        }

        if (meleeTimer > 0) meleeTimer -= Time.deltaTime;
        if (spellTimer > 0) spellTimer -= Time.deltaTime;

        if (isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (isAttackLocked)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (!isCasting)
            {
                attackLockTimer -= Time.deltaTime;
                CheckHitWindow();

                if (attackLockTimer <= 0f)
                {
                    isAttackLocked = false;
                    attackStartTime = -1f;
                    hasHitDone = false;
                }
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            currentState = State.MeleeAttack;
        }
        else if (dist <= spellDistance && spellTimer <= 0)
        {
            currentState = State.SpellAttack;
        }
        else if (dist <= chaseDistance)
        {
            currentState = State.Run;
        }
        else if (dist <= visionDistance)
        {
            currentState = State.Idle;
        }
        else
        {
            currentState = State.Rest;
        }

        switch (currentState)
        {
            case State.Rest: RestBehavior(); break;
            case State.Idle: IdleBehavior(); break;
            case State.Run: RunBehavior(); break;
            case State.MeleeAttack: MeleeAttackBehavior(); break;
            case State.SpellAttack: SpellAttackBehavior(); break;
        }
    }

    void LateUpdate()
    {
        if (isDead) return;
        float currentX = transform.localScale.x > 0 ? targetScale : -targetScale;
        transform.localScale = new Vector3(currentX, targetScale, 1f);
    }

    private void CheckHitWindow()
    {
        if (attackStartTime < 0 || player == null) return;

        float elapsed = Time.time - attackStartTime;
        float dist = Vector2.Distance(transform.position, player.position);

        if (!hasHitDone && elapsed >= hitTime)
        {
            hasHitDone = true;
            if (dist <= attackDistance && IsPlayerInFront())
            {
                TryHitPlayer(meleeDamage);
            }
        }
    }

    private bool IsPlayerInFront()
    {
        float dirToPlayer = player.position.x - transform.position.x;
        float facingDir = transform.localScale.x < 0 ? 1f : -1f;
        return Mathf.Sign(dirToPlayer) == Mathf.Sign(facingDir);
    }

    private void TryHitPlayer(float damageAmount)
    {
        McCombat mc = player.GetComponent<McCombat>();
        if (mc != null)
        {
            float originalDamage = mc.hitDamage;
            mc.hitDamage = damageAmount;
            mc.TakeHit(transform);
            mc.hitDamage = originalDamage;
        }
    }

    public override void OnHit(Transform attacker)
    {
        if (isDead) return;

        if (isAttackLocked && !isCasting)
        {
            return;
        }

        lastHitTime = Time.time;

        if (currentStunDuration <= minHitStun)
        {
            return;
        }

        isAttackLocked = false;
        isCasting = false;
        attackStartTime = -1f;
        hasHitDone = false;

        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        isStunned = true;
        anim.Play("Hurt");

        yield return new WaitForSeconds(currentStunDuration);

        isStunned = false;
        currentStunDuration -= hitStunDecay;
    }

    public override void Die()
    {
        base.Die();

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        rb.gravityScale = 0f;

        if (deathSound != null) audioSource.PlayOneShot(deathSound, deathVolume);

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        anim.Play("Death");
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }

    private void RestBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.Play("Idle");
    }

    private void IdleBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.Play("Idle");
        FlipTowardPlayer();
    }

    private void RunBehavior()
    {
        anim.Play("Walk");
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * runSpeed, rb.linearVelocity.y);
        Flip(dir.x);
    }

    private void MeleeAttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardPlayer();

        if (meleeTimer <= 0)
        {
            anim.Play("Attack");

            if (meleeSlashSound != null) audioSource.PlayOneShot(meleeSlashSound, meleeSlashVolume);

            meleeTimer = meleeCooldown;
            isAttackLocked = true;
            attackLockTimer = attackAnimDuration;

            attackStartTime = Time.time;
            hasHitDone = false;
        }
        else
        {
            anim.Play("Idle");
        }
    }

    private void SpellAttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardPlayer();

        if (spellTimer <= 0)
        {
            // Yarı can kontrolü (MobHealth'teki currentHealth değerine göre)
            if (mobHealth != null && mobHealth.CurrentHealth <= (mobHealth.maxHealth / 2f))
            {
                StartCoroutine(DoubleSpellRoutine());
            }
            else
            {
                StartCoroutine(CastThenIndependentSpellRoutine());
            }
        }
    }

    private IEnumerator CastThenIndependentSpellRoutine()
    {
        spellTimer = Random.Range(spellCooldownMin, spellCooldownMax);

        isAttackLocked = true;
        isCasting = true;

        rb.linearVelocity = Vector2.zero;
        anim.Play("Cast");

        yield return new WaitForSeconds(castAnimDuration);

        ExecuteSpell();

        isCasting = false;
        isAttackLocked = false;
    }

    private IEnumerator DoubleSpellRoutine()
    {
        spellTimer = Random.Range(spellCooldownMin, spellCooldownMax) + 1f;

        isAttackLocked = true;
        isCasting = true;
        rb.linearVelocity = Vector2.zero;

        // --- TEK SEFERLİK BÜYÜ ANİMASYONU ---
        anim.Play("Cast");

        yield return new WaitForSeconds(castAnimDuration);

        // --- BİRİNCİ BÜYÜ VURUŞU ---
        ExecuteSpell();

        // --- TAM 0.5 SANİYE BEKLEME ---
        yield return new WaitForSeconds(doubleSpellInterval);

        // --- İKİNCİ BÜYÜ VURUŞU (Animasyonsuz, anında) ---
        if (!isDead && !isStunned)
        {
            ExecuteSpell();
        }

        isCasting = false;
        isAttackLocked = false;
    }

    private void ExecuteSpell()
    {
        if (spellEffectObject != null)
        {
            // Efekti kapatıp açıyoruz ki animasyonu baştan oynasın
            spellEffectObject.SetActive(false);

            Vector2 targetPos = player.position;
            targetPos.y += spellOffsetY;

            spellEffectObject.transform.position = targetPos;
            spellEffectObject.SetActive(true);

            if (spellThunderSound != null) audioSource.PlayOneShot(spellThunderSound, spellThunderVolume);

            float distToPlayer = Vector2.Distance(spellEffectObject.transform.position, player.position);
            if (distToPlayer < 2f)
            {
                TryHitPlayer(spellDamage);
            }
        }
    }

    private void FlipTowardPlayer()
    {
        float dirX = player.position.x - transform.position.x;
        Flip(dirX);
    }

    private void Flip(float velocityX)
    {
        if (velocityX > 0.1f) transform.localScale = new Vector3(-targetScale, targetScale, 1);
        else if (velocityX < -0.1f) transform.localScale = new Vector3(targetScale, targetScale, 1);
    }
}