using UnityEngine;
using UnityEngine.Events;

namespace AbyssRun.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class AbyssPlayerController : MonoBehaviour
    {
        [Header("Movement & Jump Setup")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float wallBounceForce = 12f;
        [SerializeField] private float wallJumpUpForce = 15f;
        [SerializeField] private float airControlMultiplier = 0.8f;

        [Header("Ground & Wall Detection")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float checkDistance = 0.15f;
        [SerializeField] private Transform footPoint;
        [SerializeField] private Transform leftWallPoint;
        [SerializeField] private Transform rightWallPoint;

        [Header("Juice Events for Audio/VFX")]
        public UnityEvent onJump;
        public UnityEvent onWallBounce;
        public UnityEvent onObstacleHit;

        private Rigidbody2D rb;
        private bool isGrounded;
        private bool isTouchingWallLeft;
        private bool isTouchingWallRight;
        private int consecutiveAirBounces = 0;
        private int wallJumpCharges = 3;
        private float horizontalInput;

        public int ConsecutiveAirBounces => consecutiveAirBounces;
        public int WallJumpCharges => wallJumpCharges;

        public void RefillWallJumps()
        {
            wallJumpCharges = 3;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            ApplyMovement();
        }

        private void HandleInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (horizontalInput == 0 && Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPos = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
                horizontalInput = mouseWorldPos.x > transform.position.x ? 1f : -1f;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                TryJump();
            }
        }

        private float wallClingTimer = 0f;
        private bool wasOnWall = false;

        private void CheckCollisions()
        {
            bool wasGrounded = isGrounded;

            if (footPoint != null)
                isGrounded = Physics2D.OverlapCircle(footPoint.position, checkDistance, groundLayer);
            else
                isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);

            if (leftWallPoint != null)
                isTouchingWallLeft = Physics2D.OverlapCircle(leftWallPoint.position, checkDistance, groundLayer);
            else
                isTouchingWallLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.6f, groundLayer);

            if (rightWallPoint != null)
                isTouchingWallRight = Physics2D.OverlapCircle(rightWallPoint.position, checkDistance, groundLayer);
            else
                isTouchingWallRight = Physics2D.Raycast(transform.position, Vector2.right, 0.6f, groundLayer);

            if (isGrounded && !wasGrounded)
            {
                consecutiveAirBounces = 0;
                RefillWallJumps();
            }

            bool isOnWallNow = !isGrounded && (isTouchingWallLeft || isTouchingWallRight);
            if (isOnWallNow && !wasOnWall && rb.linearVelocity.y < 2f)
            {
                wallClingTimer = 1.2f;
            }
            wasOnWall = isOnWallNow;
        }

        public float WebDamping { get; set; } = 1f;
        public bool IsWebGlued { get; set; } = false;
        public float OriginalGravityScale { get; set; } = 3f;
        private MagneticGrapple myGrapple;

        private void Start()
        {
            myGrapple = GetComponent<MagneticGrapple>();
            OriginalGravityScale = rb.gravityScale;
        }

        private void ApplyMovement()
        {
            if (myGrapple != null && myGrapple.IsGrappling) return;

            if (IsWebGlued)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float currentControl = isGrounded ? 1f : airControlMultiplier;
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * currentControl * WebDamping, rb.linearVelocity.y);

            if (!isGrounded && ((isTouchingWallLeft && horizontalInput <= 0) || (isTouchingWallRight && horizontalInput >= 0)))
            {
                if (wallClingTimer > 0f)
                {
                    wallClingTimer -= Time.fixedDeltaTime;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
                else if (rb.linearVelocity.y < -2.5f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2.5f);
                }
            }
        }

        private void TryJump()
        {
            if (IsWebGlued)
            {
                IsWebGlued = false;
                rb.gravityScale = OriginalGravityScale;
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, jumpForce * 0.85f);
                onJump?.Invoke();
                return;
            }

            float currentJumpForce = jumpForce * Mathf.Lerp(0.4f, 1f, WebDamping);
            float currentWallBounce = wallBounceForce * Mathf.Lerp(0.5f, 1f, WebDamping);
            float currentWallJumpUp = wallJumpUpForce * Mathf.Lerp(0.5f, 1f, WebDamping);

            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentJumpForce);
                onJump?.Invoke();
                consecutiveAirBounces++;
            }
            else if (isTouchingWallLeft || isTouchingWallRight)
            {
                if (wallJumpCharges <= 0)
                {
                    return;
                }
                wallJumpCharges--;

                wallClingTimer = 0f;
                float directionX = isTouchingWallLeft ? currentWallBounce : -currentWallBounce;
                rb.linearVelocity = new Vector2(directionX, currentWallJumpUp);
                onWallBounce?.Invoke();
                consecutiveAirBounces++;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Hazard"))
            {
                Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
                rb.linearVelocity = new Vector2(knockbackDir.x * 10f, -15f);
                onObstacleHit?.Invoke();
                consecutiveAirBounces = 0;
            }
        }
    }
}