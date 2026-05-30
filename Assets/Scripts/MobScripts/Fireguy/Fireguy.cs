using UnityEngine;
using System.Collections;

public class Fireguy : BaseMob
{
    [Header("Boyut Ayarı")]
    public float targetScale = 1f;

    [Header("Ses Ayarları (Audio)")]
    public AudioClip heavyAttackSound;
    [Range(0f, 1f)] public float heavyAttackVolume = 1f;

    public AudioClip burningPassiveSound;
    [Range(0f, 1f)] public float burningPassiveVolume = 0.6f;

    private AudioSource sfxSource;
    private AudioSource passiveSource;

    [Header("Mesafe Ayarları")]
    public Transform player;
    public float visionDistance = 10f;
    public float chaseDistance = 8f;
    public float runSpeed = 3.5f;

    [Header("Normal Saldırı Ayarları")]
    public float normalDamage = 20f; // Normal saldırı hasarı eklendi
    public float normalAttackDistance = 1.5f;
    public float normalAttackCooldown = 1.5f;
    public float normalAttackAnimDuration = 0.8f;
    public float normalHitTime = 0.4f;

    [Header("Ağır Saldırı (Heavy) Ayarları")]
    public float heavyDamage = 40f; // Ağır saldırı hasarı eklendi
    public float heavyAttackDistance = 2.5f;
    public float heavyAttackCooldown = 2.5f;
    public float heavyAttackAnimDuration = 1.2f;
    public float heavyHitTime = 0.7f;

    [Header("ÖZEL: Hasar Alma (Hit) Ayarları")]
    public float baseHitStunDuration = 0.3f;
    public float hitStunDecay = 0.1f;
    public float minHitStun = 0f;
    public float stunResetTime = 2.0f;
    public float knockbackForceX = 3f;
    public float knockbackForceY = 4f;

    [Header("Ölüm Ayarları")]
    public float deathAnimDuration = 1.0f;

    private float attackTimer;
    private float currentStunDuration;
    private float lastHitTime = -10f;

    private bool isAttackLocked = false;
    private float attackLockTimer = 0f;
    private bool isStunned = false;

    private float attackStartTime = -1f;
    private bool hasHitDone = false;
    private bool isHeavyAttacking = false;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private enum State { Rest, Idle, Run, Attack }
    private State currentState;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentState = State.Rest;

        currentStunDuration = baseHitStunDuration;

        sfxSource = gameObject.AddComponent<AudioSource>();
        passiveSource = gameObject.AddComponent<AudioSource>();

        if (burningPassiveSound != null)
        {
            passiveSource.clip = burningPassiveSound;
            passiveSource.loop = true;
            passiveSource.volume = burningPassiveVolume;
            passiveSource.spatialBlend = 1f;
            passiveSource.minDistance = 3f;
            passiveSource.maxDistance = 15f;
            passiveSource.Play();
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        if (passiveSource != null) passiveSource.volume = burningPassiveVolume;

        if (Time.time - lastHitTime > stunResetTime)
        {
            currentStunDuration = baseHitStunDuration;
        }

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (isStunned) return;

        if (isAttackLocked)
        {
            attackLockTimer -= Time.deltaTime;

            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            CheckHitWindow();

            if (attackLockTimer <= 0f)
            {
                isAttackLocked = false;
                attackStartTime = -1f;
                hasHitDone = false;
                DisableHitbox();
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= normalAttackDistance) currentState = State.Attack;
        else if (dist <= chaseDistance) currentState = State.Run;
        else if (dist <= visionDistance) currentState = State.Idle;
        else currentState = State.Rest;

        switch (currentState)
        {
            case State.Rest: RestBehavior(); break;
            case State.Idle: IdleBehavior(); break;
            case State.Run: RunBehavior(); break;
            case State.Attack: AttackBehavior(); break;
        }
    }

    void LateUpdate()
    {
        if (isDead) return;

        if (!isStunned && !isAttackLocked)
        {
            float currentX = transform.localScale.x > 0 ? targetScale : -targetScale;
            transform.localScale = new Vector3(currentX, targetScale, 1f);
        }
    }

    private void CheckHitWindow()
    {
        if (attackStartTime < 0 || player == null) return;
        float elapsed = Time.time - attackStartTime;
        float dist = Vector2.Distance(transform.position, player.position);

        float currentValidDistance = isHeavyAttacking ? heavyAttackDistance : normalAttackDistance;
        float currentHitTime = isHeavyAttacking ? heavyHitTime : normalHitTime;

        // Hasar miktarını belirle
        float currentDamage = isHeavyAttacking ? heavyDamage : normalDamage;

        if (!hasHitDone && elapsed >= currentHitTime)
        {
            hasHitDone = true;
            EnableHitbox();

            if (dist <= currentValidDistance && IsPlayerInFront())
            {
                TryHitPlayer(currentDamage);
            }
        }
    }

    private bool IsPlayerInFront()
    {
        float dirToPlayer = player.position.x - transform.position.x;
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
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
        if (isAttackLocked) return;

        lastHitTime = Time.time;
        if (currentStunDuration <= minHitStun) return;

        isAttackLocked = false;
        attackStartTime = -1f;
        hasHitDone = false;

        DisableHitbox();

        StopAllCoroutines();
        StartCoroutine(CustomHitRoutine(attacker));
    }

    private IEnumerator CustomHitRoutine(Transform attacker)
    {
        isStunned = true;

        if (attacker != null)
        {
            float knockDirX = attacker.position.x > transform.position.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(knockDirX * knockbackForceX, knockbackForceY);
        }

        if (sr != null)
        {
            float flashDuration = 0.2f;
            float flashInterval = 0.06f;
            float timer = 0f;
            bool isWhitish = false;

            while (timer < flashDuration)
            {
                sr.color = isWhitish ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                isWhitish = !isWhitish;

                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }
            sr.color = Color.white;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        float remainingStun = currentStunDuration - 0.2f;
        if (remainingStun > 0)
        {
            yield return new WaitForSeconds(remainingStun);
        }

        isStunned = false;
        currentStunDuration -= hitStunDecay;
    }

    public override void Die()
    {
        base.Die();
        StopAllCoroutines();

        if (passiveSource != null) passiveSource.Stop();

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (sr != null) sr.color = Color.white;

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        bool isFacingLeft = transform.localScale.x < 0;
        transform.localScale = new Vector3(Mathf.Abs(targetScale), targetScale, 1f);

        if (isFacingLeft) anim.Play("Death_left");
        else anim.Play("Death_right");

        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }

    private void RestBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.Play("idle_right");
    }

    private void IdleBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardPlayer();
        anim.Play("idle_right");
    }

    private void RunBehavior()
    {
        anim.Play("run");
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * runSpeed, rb.linearVelocity.y);
        Flip(dir.x);
    }

    private void AttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardPlayer();

        if (attackTimer <= 0)
        {
            if (Random.value < 0.3f)
            {
                anim.Play("heavy_attack_right");
                isHeavyAttacking = true;
                if (attackHitbox != null) attackHitbox.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

                if (heavyAttackSound != null) sfxSource.PlayOneShot(heavyAttackSound, heavyAttackVolume);

                attackTimer = heavyAttackCooldown;
                attackLockTimer = heavyAttackAnimDuration;
            }
            else
            {
                anim.Play("Attack_Right");
                isHeavyAttacking = false;
                if (attackHitbox != null) attackHitbox.transform.localScale = Vector3.one;

                attackTimer = normalAttackCooldown;
                attackLockTimer = normalAttackAnimDuration;
            }

            isAttackLocked = true;
            attackStartTime = Time.time;
            hasHitDone = false;
        }
        else
        {
            anim.Play("idle_right");
        }
    }

    private void FlipTowardPlayer()
    {
        float dirX = player.position.x - transform.position.x;
        Flip(dirX);
    }

    private void Flip(float velocityX)
    {
        if (velocityX > 0.1f) transform.localScale = new Vector3(targetScale, targetScale, 1);
        else if (velocityX < -0.1f) transform.localScale = new Vector3(-targetScale, targetScale, 1);
    }
}