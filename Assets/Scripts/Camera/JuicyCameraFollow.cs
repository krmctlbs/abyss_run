using UnityEngine;
using AbyssRun.Player;

namespace AbyssRun.Camera
{
    public class JuicyCameraFollow : MonoBehaviour
    {
        [Header("Target & Speed")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

        [Header("Shaft Constraints")]
        [SerializeField] private bool lockXPosition = true;
        [SerializeField] private float lockedX = 0f;
        [SerializeField] private float minFloorY = 0f;

        [Header("Juice: Screen Shake")]
        [SerializeField] private float shakeDecay = 12f;
        private float shakeIntensity = 0f;
        private Vector3 currentVelocity;

        public static JuicyCameraFollow Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
                if (playerObj != null) target = playerObj.transform;
            }

            if (target != null)
            {
                var playerController = target.GetComponent<AbyssPlayerController>();
                if (playerController != null)
                {
                    playerController.onJump.AddListener(() => TriggerShake(0.02f));
                    playerController.onWallBounce.AddListener(() => TriggerShake(0.05f));
                    playerController.onObstacleHit.AddListener(() => TriggerShake(0.25f));
                }

                var grapple = target.GetComponent<MagneticGrapple>();
                if (grapple != null)
                {
                    grapple.onGrappleBoost.AddListener(() => TriggerShake(0.12f));
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (Time.timeScale == 0f)
            {
                shakeIntensity = 0f;
            }

            float targetX = lockXPosition ? lockedX : (target.position.x + offset.x);
            float targetY = Mathf.Max(minFloorY, target.position.y + offset.y);
            Vector3 goalPos = new Vector3(targetX, targetY, offset.z);

            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, goalPos, ref currentVelocity, smoothTime);

            if (shakeIntensity > 0.001f && Time.timeScale > 0f)
            {
                Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
                smoothedPos += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, Time.unscaledDeltaTime * shakeDecay);
            }

            transform.position = smoothedPos;
        }

        public void TriggerShake(float intensity)
        {
            if (Time.timeScale == 0f) return;
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }
    }
}
