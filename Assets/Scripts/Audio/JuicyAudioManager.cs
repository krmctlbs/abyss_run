using UnityEngine;
using AbyssRun.Player;
using AbyssRun.Managers;

namespace AbyssRun.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class JuicyAudioManager : MonoBehaviour
    {
        public static JuicyAudioManager Instance { get; private set; }

        [Header("Sound Clips")]
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip wallBounceClip;
        [SerializeField] private AudioClip grappleConnectClip;
        [SerializeField] private AudioClip grappleBoostClip;
        [SerializeField] private AudioClip hazardShockClip;
        [SerializeField] private AudioClip webStickyClip;

        [Header("Pitch Modulation")]
        [SerializeField] private float basePitch = 1.0f;
        [SerializeField] private float pitchStep = 0.07f;
        [SerializeField] private float maxPitch = 1.6f;
        [SerializeField] private float randomPitchJitter = 0.05f;

        [Header("References")]
        [SerializeField] private AbyssPlayerController playerController;
        [SerializeField] private SpeedrunGameManager gameManager;
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<AbyssPlayerController>();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<SpeedrunGameManager>();

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            if (playerController != null)
            {
                playerController.onJump.AddListener(PlayJump);
                playerController.onWallBounce.AddListener(PlayWallBounce);
                playerController.onObstacleHit.AddListener(PlayHazardShock);

                MagneticGrapple grapple = playerController.GetComponent<MagneticGrapple>();
                if (grapple != null)
                {
                    grapple.onGrappleConnect.AddListener(PlayGrappleConnect);
                    grapple.onGrappleBoost.AddListener(PlayGrappleBoost);
                }
            }
        }

        public void PlayJump()
        {
            ApplyComboPitch(true);
            PlayClip(jumpClip, 0.85f);
        }

        public void PlayWallBounce()
        {
            ApplyComboPitch(true);
            PlayClip(wallBounceClip, 1.0f);
        }

        public void PlayGrappleConnect()
        {
            sfxSource.pitch = 1.15f + Random.Range(-0.04f, 0.04f);
            PlayClip(grappleConnectClip, 0.9f);
        }

        public void PlayGrappleBoost()
        {
            ApplyComboPitch(false);
            PlayClip(grappleBoostClip, 1.2f);
        }

        public void PlayHazardShock()
        {
            sfxSource.pitch = 0.7f + Random.Range(-0.05f, 0.05f);
            PlayClip(hazardShockClip, 1.1f);
        }

        public void PlayWebSticky()
        {
            sfxSource.pitch = 0.9f + Random.Range(-0.05f, 0.05f);
            PlayClip(webStickyClip, 0.9f);
        }

        private void ApplyComboPitch(bool addJitter)
        {
            int bounces = playerController != null ? playerController.ConsecutiveAirBounces : 0;
            float targetPitch = basePitch + (bounces * pitchStep);
            if (addJitter)
            {
                targetPitch += Random.Range(-randomPitchJitter, randomPitchJitter);
            }
            sfxSource.pitch = Mathf.Clamp(targetPitch, 0.7f, maxPitch);
        }

        private void PlayClip(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volumeScale);
            }
        }
    }
}
