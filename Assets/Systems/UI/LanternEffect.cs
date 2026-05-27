using UnityEngine;

[DisallowMultipleComponent]
public class OminousLanternEffect : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light glowLight;
    [SerializeField] private Color glowColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float glowIntensity = 2.6f;
    [SerializeField] private float glowRange = 8f;

    [Header("Embers")]
    [SerializeField] private Color emberColor = new Color(0.45f, 0f, 0f, 0.9f);
    [SerializeField] private float emberRate = 1.25f;
    [SerializeField] private float emberLifetime = 2.5f;
    [SerializeField] private float emberRiseSpeed = 0.18f;
    [SerializeField] private float emberStartSize = 0.035f;

    private const string EmberObjectName = "LanternRedEmbers";
    private const string ParticleMaterialName = "RuntimeDarkRedLanternParticle";

    private static Material particleMaterial;
    private ParticleSystem embers;

    private void Awake()
    {
        ConfigureLight();
        ConfigureEmbers();
    }

    private void OnEnable()
    {
        ConfigureLight();
        ConfigureEmbers();

        if (embers != null && !embers.isPlaying)
        {
            embers.Play();
        }
    }

    private void OnValidate()
    {
        ConfigureLight();
    }

    private void ConfigureLight()
    {
        if (glowLight == null)
        {
            glowLight = GetComponentInChildren<Light>(true);
        }

        if (glowLight == null)
        {
            return;
        }

        glowLight.type = LightType.Point;
        glowLight.color = glowColor;
        glowLight.intensity = glowIntensity;
        glowLight.range = glowRange;
        glowLight.shadows = LightShadows.None;
        glowLight.renderMode = LightRenderMode.Auto;
    }

    private void ConfigureEmbers()
    {
        Transform emberTransform = transform.Find(EmberObjectName);
        if (emberTransform == null)
        {
            GameObject emberObject = new GameObject(EmberObjectName);
            emberTransform = emberObject.transform;
            emberTransform.SetParent(transform, false);
            emberTransform.localPosition = new Vector3(0f, 0.28f, 0f);
        }

        embers = emberTransform.GetComponent<ParticleSystem>();
        if (embers == null)
        {
            embers = emberTransform.gameObject.AddComponent<ParticleSystem>();
        }

        if (embers.isPlaying || embers.isEmitting)
        {
            embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ParticleSystemRenderer particleRenderer = emberTransform.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer != null)
        {
            Mesh particleMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            if (particleMesh != null)
            {
                particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                particleRenderer.mesh = particleMesh;
            }
            else
            {
                particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            particleRenderer.sharedMaterial = GetParticleMaterial();
        }

        ParticleSystem.MainModule main = embers.main;
        main.loop = true;
        main.playOnAwake = true;
        main.duration = 4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(emberLifetime * 0.75f, emberLifetime * 1.25f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(emberStartSize * 0.45f, emberStartSize);
        main.startColor = new ParticleSystem.MinMaxGradient(emberColor, new Color(0.18f, 0f, 0f, 0.35f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = 16;

        ParticleSystem.EmissionModule emission = embers.emission;
        emission.enabled = true;
        emission.rateOverTime = emberRate;

        ParticleSystem.ShapeModule shape = embers.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        ParticleSystem.VelocityOverLifetimeModule velocity = embers.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(emberRiseSpeed * 0.45f, emberRiseSpeed);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = embers.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(emberColor, 0f),
                new GradientColorKey(new Color(0.16f, 0f, 0f, 1f), 1f),
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.85f, 0.2f),
                new GradientAlphaKey(0f, 1f),
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = embers.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
    }
    private static Material GetParticleMaterial()
    {
        if (particleMaterial != null)
        {
            return particleMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        particleMaterial = new Material(shader)
        {
            name = ParticleMaterialName,
            color = new Color(0.28f, 0f, 0f, 1f),
        };

        SetMaterialColor(particleMaterial, "_BaseColor", particleMaterial.color);
        SetMaterialColor(particleMaterial, "_Color", particleMaterial.color);
        SetMaterialColor(particleMaterial, "_TintColor", particleMaterial.color);
        SetMaterialColor(particleMaterial, "_EmissionColor", new Color(0.18f, 0f, 0f, 1f));

        return particleMaterial;
    }

    private static void SetMaterialColor(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

}
