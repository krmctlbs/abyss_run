using UnityEngine;
using UnityEngine.Events;

namespace AbyssRun.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MagneticGrapple : MonoBehaviour
    {
        [Header("Grapple Settings")]
        [SerializeField] private float maxGrappleDistance = 3f;
        [SerializeField] private float pullSpeed = 15f;
        [SerializeField] private float boostLaunchMultiplier = 1.5f;
        [SerializeField] private LayerMask grappleLayer;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Events")]
        public UnityEvent onGrappleConnect;
        public UnityEvent onGrappleBoost;

        private Rigidbody2D rb;
        private AbyssPlayerController playerController;
        private Vector2 connectedPoint;
        private bool isGrappling;
        private float grappleDuration = 0f;

        public bool IsGrappling => isGrappling;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerController = GetComponent<AbyssPlayerController>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private void Update()
        {
            HandleGrappleInput();
            UpdateGrappleLine();
        }

        private void FixedUpdate()
        {
            if (isGrappling)
            {
                ApplyPullForce();
            }
        }

        private void HandleGrappleInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                TryShootGrapple();
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                if (isGrappling)
                {
                    ReleaseAndBoost();
                }
            }
        }

        private void TryShootGrapple()
        {
            Collider2D[] potentialTargets;
            if (grappleLayer.value != 0)
            {
                potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxGrappleDistance, grappleLayer);
            }
            else
            {
                potentialTargets = Physics2D.OverlapCircleAll(transform.position, maxGrappleDistance);
            }

            Collider2D bestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var col in potentialTargets)
            {
                if (col == null || col.gameObject == gameObject) continue;

                if (grappleLayer.value == 0)
                {
                    string nameLower = col.name.ToLower();
                    if (!col.CompareTag("Grapple") && !nameLower.Contains("grapple") && !nameLower.Contains("anchor"))
                        continue;
                }

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist <= maxGrappleDistance && dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = col;
                }
            }

            if (bestTarget != null)
            {
                isGrappling = true;
                grappleDuration = 0f;
                connectedPoint = bestTarget.transform.position;

                if (lineRenderer != null)
                {
                    lineRenderer.enabled = true;
                }

                if (playerController != null && playerController.IsWebGlued)
                {
                    playerController.IsWebGlued = false;
                    rb.gravityScale = playerController.OriginalGravityScale;
                }

                onGrappleConnect?.Invoke();
            }
        }

        private void ApplyPullForce()
        {
            grappleDuration += Time.fixedDeltaTime;
            Vector2 pullDirection = (connectedPoint - (Vector2)transform.position).normalized;
            rb.linearVelocity = pullDirection * pullSpeed;

            if (Vector2.Distance(transform.position, connectedPoint) < 0.8f || grappleDuration >= 0.5f)
            {
                ReleaseAndBoost();
            }
        }

        private void ReleaseAndBoost()
        {
            isGrappling = false;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            float launchSpeed = pullSpeed * boostLaunchMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.7f, launchSpeed);

            playerController?.RefillWallJumps();
            onGrappleBoost?.Invoke();
        }

        private void UpdateGrappleLine()
        {
            if (!isGrappling || lineRenderer == null) return;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, connectedPoint);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.8f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxGrappleDistance);
        }
    }
}
