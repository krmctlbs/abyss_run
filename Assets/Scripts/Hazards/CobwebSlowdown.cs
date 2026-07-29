using UnityEngine;
using AbyssRun.Player;
using AbyssRun.Camera;

namespace AbyssRun.Hazards
{
    [RequireComponent(typeof(Collider2D))]
    public class CobwebSlowdown : MonoBehaviour
    {
        [Header("Cobweb")]
        [SerializeField] private float dragDamping = 0.25f;
        [SerializeField] private float exitBoostForce = 3f;
        [SerializeField] private int staminaDrain = 1;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private float originalGravityScale = 3f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                AbyssPlayerController playerController = other.GetComponent<AbyssPlayerController>();

                if (playerRb != null && playerController != null)
                {
                    originalGravityScale = playerController.OriginalGravityScale;
                    playerController.IsWebGlued = true;
                    playerController.WebDamping = dragDamping;
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.gravityScale = 0f;
                }

                JuicyCameraFollow.Instance?.TriggerShake(0.06f);
                Audio.JuicyAudioManager.Instance?.PlayWebSticky();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                AbyssPlayerController playerController = other.GetComponent<AbyssPlayerController>();
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerController != null && playerController.IsWebGlued && playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                AbyssPlayerController playerController = other.GetComponent<AbyssPlayerController>();
                if (playerController != null)
                {
                    playerController.IsWebGlued = false;
                    playerController.WebDamping = 1f;
                }

                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerController != null)
                {
                    playerRb.gravityScale = playerController.OriginalGravityScale;
                    if (playerRb.linearVelocity.y > 0)
                    {
                        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, playerRb.linearVelocity.y + exitBoostForce);
                    }
                }
            }
        }
    }
}
