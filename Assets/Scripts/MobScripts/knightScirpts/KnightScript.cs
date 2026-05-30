using UnityEngine;
using System.Collections;

public class KnightScript : BaseMob
{
    [Header("Boyut Ayarı")]
    public float targetScale = 3.5f;

    [Header("Savaş ve Görüş Ayarları")]
    public Transform player;
    public float visionDistance = 10f;
    public float chaseDistance = 7f;
    public float attackDistance = 1.5f;
    public float runSpeed = 4f;
    public float attackCooldown = 1.5f;

    [Header("Hasar Ayarları")]
    public float attackDamage = 10f; // Şövalye hasarı eklendi

    [Header("Animasyon ve Vuruş Zamanlamaları")]
    public float attackAnimDuration = 0.8f;
    public float firstHitTime = 0.20f;
    public float secondHitTime = 0.55f;
    public float shieldHitAnimDuration = 0.3f;
    public float shieldDuration = 1.0f;

    [Header("Stun Direnci (Anti-Spam) Ayarları")]
    public float baseHitStunDuration = 0.2f;
    public float hitStunDecay = 0.1f;
    public float minHitStun = 0f;
    public float stunResetTime = 2.0f;

    [Header("Hitbox Pozisyon Ayarı")]
    public float hitboxOffsetX = 0.17f;

    [Header("Ses Efektleri ve Ses Seviyeleri (SFX Volume)")]
    public AudioSource audioSource;

    public AudioClip walkSound1;
    [Range(0f, 1f)] public float walkSound1Volume = 0.5f;

    public AudioClip walkSound2;
    [Range(0f, 1f)] public float walkSound2Volume = 0.5f;

    public AudioClip attackSound1;
    [Range(0f, 1f)] public float attackSound1Volume = 0.8f;

    public AudioClip attackSound2;
    [Range(0f, 1f)] public float attackSound2Volume = 0.8f;

    public AudioClip shieldBlockSound1;
    [Range(0f, 1f)] public float shieldBlockSound1Volume = 0.9f;

    public AudioClip shieldBlockSound2;
    [Range(0f, 1f)] public float shieldBlockSound2Volume = 0.9f;

    public AudioClip hitSound;         // Knight_hit
    [Range(0f, 1f)] public float hitSoundVolume = 0.8f;

    public AudioClip defeatSound;      // Knight_defeat
    [Range(0f, 1f)] public float defeatSoundVolume = 1f;

    [Header("Yürüme Sesi Ayarları")]
    public float stepInterval = 0.4f;
    private float stepTimer;

    // Zamanlayıcılar ve sayaçlar
    private float attackTimer;
    private float attackStartTime = -1f;
    private bool firstHitDone = false;
    private bool secondHitDone = false;

    // Anti-Spam Sayaçları
    private float currentStunDuration;
    private float lastHitTime = -10f;

    private bool isShielding = false;
    private bool isShieldHit = false;
    private float shieldTimer = 0f;

    private bool isAttackLocked = false;
    private float attackLockTimer = 0f;

    private bool isStunned = false;

    private Rigidbody2D rb;
    private Animator anim;

    private Coroutine shieldCoroutine;
    private Coroutine shieldHitCoroutine;
    private Coroutine delayedShieldCoroutine;

    public override bool IsShielding => isShielding;

    private enum State { Rest, Idle, Run, Attack }
    private State currentState;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        currentState = State.Rest;
        currentStunDuration = baseHitStunDuration;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        if (Time.time - lastHitTime > stunResetTime)
        {
            currentStunDuration = baseHitStunDuration;
        }

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (isStunned) return;

        if (isShielding)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (!isShieldHit) shieldTimer -= Time.deltaTime;

            if (shieldTimer <= 0f)
            {
                isShielding = false;
                isShieldHit = false;
                attackTimer = 0f;
            }
            return;
        }

        if (isAttackLocked)
        {
            attackLockTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            CheckHitWindows();

            if (attackLockTimer <= 0f)
            {
                isAttackLocked = false;
                DisableHitbox();
                attackStartTime = -1f;
                firstHitDone = false;
                secondHitDone = false;
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackDistance) currentState = State.Attack;
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
        float currentX = transform.localScale.x > 0 ? targetScale : -targetScale;
        transform.localScale = new Vector3(currentX, targetScale, 1f);
    }

    public override void Die()
    {
        base.Die();
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        PlaySound(defeatSound, defeatSoundVolume);

        anim.Play("rest");
        this.enabled = false;
    }

    public override void OnHit(Transform attacker)
    {
        if (isDead) return;
        if (isAttackLocked) return;

        lastHitTime = Time.time;

        if (currentStunDuration <= minHitStun) return;

        if (isStunned)
        {
            if (delayedShieldCoroutine != null) StopCoroutine(delayedShieldCoroutine);
            delayedShieldCoroutine = StartCoroutine(DelayedShield());
            return;
        }

        if (isShielding)
        {
            if (IsAttackerInFront(attacker))
            {
                PlayRandomSound(shieldBlockSound1, shieldBlockSound1Volume, shieldBlockSound2, shieldBlockSound2Volume);

                shieldTimer = shieldDuration;
                if (!isShieldHit)
                {
                    if (shieldHitCoroutine != null) StopCoroutine(shieldHitCoroutine);
                    shieldHitCoroutine = StartCoroutine(ShieldHitAnim());
                }

                if (Random.value > 0.5f)
                {
                    if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
                    if (shieldHitCoroutine != null) StopCoroutine(shieldHitCoroutine);
                    isShielding = false;
                    isShieldHit = false;
                    attackTimer = 0f;
                }
            }
            return;
        }

        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
        if (shieldHitCoroutine != null) StopCoroutine(shieldHitCoroutine);
        if (delayedShieldCoroutine != null) StopCoroutine(delayedShieldCoroutine);

        isAttackLocked = false;
        isShielding = false;
        isShieldHit = false;
        isStunned = false;
        DisableHitbox();
        attackStartTime = -1f;
        firstHitDone = false;
        secondHitDone = false;

        delayedShieldCoroutine = StartCoroutine(DelayedShield());
    }

    private IEnumerator DelayedShield()
    {
        isStunned = true;

        PlaySound(hitSound, hitSoundVolume);

        anim.Play("hit");

        yield return new WaitForSeconds(currentStunDuration);

        isStunned = false;
        currentStunDuration -= hitStunDecay;

        shieldCoroutine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        isShielding = true;
        shieldTimer = shieldDuration;
        anim.Play("shield_idle");

        while (isShielding)
            yield return null;
    }

    private IEnumerator ShieldHitAnim()
    {
        isShieldHit = true;
        anim.Play("shield_hit");
        yield return new WaitForSeconds(shieldHitAnimDuration);
        anim.Play("shield_idle");
        isShieldHit = false;
    }

    public override bool IsAttackerInFront(Transform attacker)
    {
        float dirToAttacker = attacker.position.x - transform.position.x;
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        return Mathf.Sign(dirToAttacker) == Mathf.Sign(facingDir);
    }

    private void CheckHitWindows()
    {
        if (attackStartTime < 0 || player == null) return;
        float elapsed = Time.time - attackStartTime;
        float dist = Vector2.Distance(transform.position, player.position);

        if (!firstHitDone && elapsed >= firstHitTime)
        {
            firstHitDone = true;
            PlaySound(attackSound1, attackSound1Volume);

            if (dist <= attackDistance && IsPlayerInFront()) TryHitPlayer(attackDamage);
        }
        if (!secondHitDone && elapsed >= secondHitTime)
        {
            secondHitDone = true;
            PlaySound(attackSound2, attackSound2Volume);

            if (dist <= attackDistance && IsPlayerInFront()) TryHitPlayer(attackDamage);
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

    private void RestBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.Play("rest");
        DisableHitbox();
    }

    private void IdleBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.Play("IDLE");
        FlipTowardPlayer();
        DisableHitbox();
    }

    private void RunBehavior()
    {
        anim.Play("run");
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * runSpeed, rb.linearVelocity.y);
        Flip(dir.x);
        DisableHitbox();

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayRandomSound(walkSound1, walkSound1Volume, walkSound2, walkSound2Volume);
            stepTimer = stepInterval;
        }
    }

    private void AttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardPlayer();

        if (attackTimer <= 0)
        {
            anim.Play("attack");
            attackTimer = attackCooldown;
            isAttackLocked = true;
            attackLockTimer = attackAnimDuration;
            attackStartTime = Time.time;
            firstHitDone = false;
            secondHitDone = false;

            if (attackHitbox != null)
            {
                float dir = transform.localScale.x > 0 ? 1f : -1f;
                attackHitbox.transform.localPosition = new Vector3(
                    hitboxOffsetX * dir,
                    attackHitbox.transform.localPosition.y, 0f);
            }
        }
        else
        {
            anim.Play("IDLE");
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

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void PlayRandomSound(AudioClip clip1, float vol1, AudioClip clip2, float vol2)
    {
        if (audioSource != null)
        {
            bool chooseFirst = Random.value > 0.5f;
            AudioClip clipToPlay = chooseFirst ? clip1 : clip2;
            float volumeToUse = chooseFirst ? vol1 : vol2;

            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay, volumeToUse);
            }
        }
    }
}