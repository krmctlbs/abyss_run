using UnityEngine;
using UnityEngine.Events;
using AbyssRun.Managers;

namespace AbyssRun.Hazards
{
    [RequireComponent(typeof(Collider2D))]
    public class AbyssVictoryZone : MonoBehaviour
    {
        public UnityEvent onSummitReached;
        private bool alreadyTriggered = false;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (alreadyTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<Player.AbyssPlayerController>() != null)
            {
                alreadyTriggered = true;

                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }

                if (SpeedrunGameManager.Instance != null)
                {
                    SpeedrunGameManager.Instance.TriggerVictory();
                }

                onSummitReached?.Invoke();
            }
        }
    }
}
