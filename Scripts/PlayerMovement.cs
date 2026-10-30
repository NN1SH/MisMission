using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;
    public ParticleSystem smokeFX;
    public ParticleSystem speedFX;
    BoxCollider2D playerCollider;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;
    float speedMultiplier = 1f;

    [Header("Dash")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.2f;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public int maxJumps = 2;
    int jumpremain;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Gravity")]
    public float gravityScale = 2f;
    public float maxFallSpeed = 17f;
    public float fallMultiplier = 2f;

    [Header("WallCheck")]
    public Transform wallCheck;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.5f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding;
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumptime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpForce = new Vector2(5f, 5f);

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        SpeedItem.OnSpeedCollected += HandleSpeedBoost;
        GameController.OnReset += ResetMovement; // เพิ่มการสมัคร OnReset
    }

    void OnDestroy()
    {
        SpeedItem.OnSpeedCollected -= HandleSpeedBoost;
        GameController.OnReset -= ResetMovement; // ยกเลิกการสมัคร
    }

    private void ResetMovement()
    {
        isWallSliding = false;
        isWallJumping = false;
        wallJumpTimer = 0f;
        jumpremain = maxJumps;
        rb.velocity = Vector2.zero;
        Debug.Log("PlayerMovement: ResetMovement called");
    }

    void HandleSpeedBoost(float speedBoost)
    {
        StartCoroutine(SpeedBoostCoroutine(speedBoost));
    }

    private IEnumerator SpeedBoostCoroutine(float speedBoost)
    {
        speedMultiplier = speedBoost;
        speedFX.Play();
        yield return new WaitForSeconds(2f); // Duration of the speed boost
        speedMultiplier = 1f; // Reset to normal speed
        speedFX.Stop();
    }

    void Update()
    {
        GroundCheck();
        Gravity();
        WallSlide();
        WallJump();

        if (!isWallJumping && !isDashing)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed * speedMultiplier, rb.velocity.y);
            Flip();
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetFloat("magnitude", rb.velocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);

        // Debug สถานะ
        Debug.Log($"isGrounded: {isGrounded}, isWallSliding: {isWallSliding}, isWallJumping: {isWallJumping}, wallJumpTimer: {wallJumpTimer}");
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && isOnPlatform && playerCollider.enabled)
        {
            StartCoroutine(DisablePlayerCollider(0.25f));
        }
    }

    private IEnumerator DisablePlayerCollider(float disableTime)
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        playerCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
    }

    private IEnumerator DashCoroutine()
    {
        Physics2D.IgnoreLayerCollision(7, 8, true);
        canDash = false;
        isDashing = true;
        SoundEffectManager.Play("Dash");
        trailRenderer.emitting = true;
        float dashDirection = isFacingRight ? 1f : -1f;

        rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = new Vector2(0f, rb.velocity.y);
        isDashing = false;
        trailRenderer.emitting = false;
        Physics2D.IgnoreLayerCollision(7, 8, false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpremain > 0)
        {
            if (context.performed)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpremain--;
                JumpFX();
            }
            else if (context.canceled)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                jumpremain--;
                JumpFX();
            }
        }

        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            wallJumpTimer = 0f;
            JumpFX();

            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumptime);
        }
    }

    private void JumpFX()
    {
        animator.SetTrigger("jump");
        SoundEffectManager.Play("Jump");
        smokeFX.Play();
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer))
        {
            jumpremain = maxJumps;
            isGrounded = true;
            // รีเซ็ตสถานะ Wall Jump เมื่อลงพื้น
            if (isWallJumping || isWallSliding)
            {
                isWallJumping = false;
                isWallSliding = false;
                wallJumpTimer = 0f;
                CancelInvoke(nameof(CancelWallJump));
                Debug.Log("GroundCheck: Reset WallJump and WallSlide states");
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);
    }

    private void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void WallSlide()
    {
        if (!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false; // ป้องกัน isWallJumping ค้างเมื่อเริ่ม WallSlide
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumptime;
            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0f && !isWallJumping)
            {
                isWallJumping = false;
                CancelInvoke(nameof(CancelWallJump));
            }
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
        Debug.Log("CancelWallJump called");
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1;
            transform.localScale = ls;
            speedFX.transform.localScale = ls;

            if (rb.velocity.y == 0)
            {
                smokeFX.Play();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}