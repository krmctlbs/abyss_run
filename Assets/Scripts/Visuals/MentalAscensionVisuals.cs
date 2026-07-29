using UnityEngine;
using AbyssRun.Managers;

namespace AbyssRun.Visuals
{
    public class MentalAscensionVisuals : MonoBehaviour
    {
        [Header("Altitude & Gradient")]
        [SerializeField] private float maxAscensionHeight;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem auraParticles;

        [Header("Color Progression")]
        [SerializeField] private Gradient mentalAscensionGradient;

        private float startY;
        private float currentProgress;

        public float CurrentClarityPercentage => currentProgress * 100f;

        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
            if (playerTransform == null) playerTransform = transform;
            startY = playerTransform.position.y;

            if (mentalAscensionGradient == null || mentalAscensionGradient.colorKeys.Length <= 1)
            {
                SetupDefaultMentalGradient();
            }
        }

        private void SetupDefaultMentalGradient()
        {
            mentalAscensionGradient = new Gradient();

            GradientColorKey[] colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(new Color(0.08f, 0.04f, 0.15f, 1f), 0.0f);  
            colorKeys[1] = new GradientColorKey(new Color(0.35f, 0.30f, 0.50f, 1f), 0.35f); 
            colorKeys[2] = new GradientColorKey(new Color(0.65f, 0.80f, 0.95f, 1f), 0.70f); 
            colorKeys[3] = new GradientColorKey(new Color(1f, 0.96f, 0.82f, 1f), 1.0f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(0.9f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            mentalAscensionGradient.SetKeys(colorKeys, alphaKeys);
        }

        private void Update()
        {
            if (playerTransform == null) return;
            float currentHeight = Mathf.Max(0f, (playerTransform.position.y - startY) * 1.5f);
            currentProgress = Mathf.Clamp01(currentHeight / maxAscensionHeight);

            Color currentColor = mentalAscensionGradient.Evaluate(currentProgress);
            if (targetRenderer != null)
            {
                targetRenderer.color = currentColor;
            }
            TrailRenderer[] allTrails = GetComponentsInChildren<TrailRenderer>();
            foreach (var trail in allTrails)
            {
                Gradient grad = trail.colorGradient;
                if (grad != null && grad.alphaKeys.Length > 0)
                {
                    GradientColorKey[] colorKeys = new GradientColorKey[] { 
                        new GradientColorKey(currentColor, 0f), 
                        new GradientColorKey(currentColor, 1f) 
                    };
                    grad.SetKeys(colorKeys, grad.alphaKeys);
                    trail.colorGradient = grad;
                }
                else
                {
                    trail.startColor = currentColor;
                    trail.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
                }
            }
            ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in allParticles)
            {
                var mainModule = ps.main;
                mainModule.startColor = currentColor;

                var trailModule = ps.trails;
                if (trailModule.enabled)
                {
                    trailModule.colorOverLifetime = new ParticleSystem.MinMaxGradient(currentColor);
                }

                var colModule = ps.colorOverLifetime;
                if (colModule.enabled)
                {
                    colModule.color = new ParticleSystem.MinMaxGradient(new Color(currentColor.r, currentColor.g, currentColor.b, 1f));
                }
            }
        }
    }
}
