using UnityEngine;
using System.Collections;

public class McMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 3f;
    [SerializeField] private LayerMask wallLayer;

    [Header("After-Image Ayarları")]
    public float afterImageInterval = 0.12f;
    public float afterImageStartAlpha = 0.7f;
    public float afterImageFadeSpeed = 4f;

    [Header("Mob Kutusu (İçinden Geçmek İçin)")]
    public Transform mobsContainer;

    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip dashSound;

    private float nextDashTime = 0f;
    public bool isDashing = false;
    public bool lastDirLeft = false;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer sr;
    private McCombat combat;

    // --- MERDIVEN SİSTEMİ ---
    private int _ladderCount = 0;                        // Kaç merdiven zone'undayız
    public bool IsOnLadder => _ladderCount > 0;          // Herhangi birindeyse true
    public void AddLadder() => _ladderCount++;         // LadderZone çağırır
    public void RemoveLadder() => _ladderCount = Mathf.Max(0, _ladderCount - 1);

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        combat = GetComponent<McCombat>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (anim == null || rb == null) return;

        // --- YENİ EKLENEN SATIR: Ölü isek hiçbir harekete izin verme ---
        if (combat != null && combat.isDead) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (lastDirLeft) anim.Play("idle_left");
            else anim.Play("idle_right");
            return;
        }

        if (isDashing) return;
        if (combat != null && combat.IsHit) return;
        if (combat != null && combat.IsMovementLocked) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        // --- ANIMATION CANCEL (İPTAL) KONTROLÜ ---
        if (combat != null && combat.IsAttacking)
        {
            // Eğer sola vururken sağa basarsa VEYA sağa vururken sola basarsa saldırıyı iptal et
            if ((combat.AttackDirLeft && moveInput > 0) || (!combat.AttackDirLeft && moveInput < 0))
            {
                combat.CancelAttack();
            }
        }

        // isGrounded VEYA merdivende → zıpla (merdivende sınırsız)
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            && (isGrounded || IsOnLadder))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (moveInput < 0) lastDirLeft = true;
        else if (moveInput > 0) lastDirLeft = false;

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime && moveInput != 0)
        {
            // Dash atarsak da saldırı animasyonunu anında iptal edelim
            if (combat != null && combat.IsAttacking) combat.CancelAttack();

            StartCoroutine(Dash(moveInput));
            return;
        }

        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            if (combat == null || !combat.IsAttacking)
            {
                if (lastDirLeft) anim.Play("move_left");
                else anim.Play("move_right");
            }
        }
        else
        {
            if (moveInput == 0)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (combat == null || !combat.IsAttacking)
                {
                    if (lastDirLeft) anim.Play("idle_left");
                    else anim.Play("idle_right");
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

                if (combat == null || !combat.IsAttacking)
                {
                    if (moveInput < 0) anim.Play("move_left");
                    else anim.Play("move_right");
                }
            }
        }
    }

    private void SpawnAfterImage()
    {
        if (sr == null || sr.sprite == null) return;

        GameObject ghost = new GameObject("AfterImage");
        ghost.AddComponent<SpriteRenderer>();
        AfterImage ai = ghost.AddComponent<AfterImage>();
        ai.Init(
            sr.sprite,
            transform.position,
            transform.localScale,
            afterImageStartAlpha,
            afterImageFadeSpeed
        );
    }

    private IEnumerator Dash(float direction)
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        if (direction < 0) anim.Play("dash_left");
        else anim.Play("dash_right");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        if (mobsContainer != null && playerCollider != null)
        {
            Collider2D[] tumMoblar = mobsContainer.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D mob in tumMoblar)
                Physics2D.IgnoreCollision(playerCollider, mob, true);
        }

        float elapsed = 0f;
        float nextImageTime = 0f;

        while (elapsed < dashDuration)
        {
            if (elapsed >= nextImageTime)
            {
                SpawnAfterImage();
                nextImageTime += afterImageInterval;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mobsContainer != null && playerCollider != null)
        {
            Collider2D[] tumMoblar = mobsContainer.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D mob in tumMoblar)
                Physics2D.IgnoreCollision(playerCollider, mob, false);
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }
}