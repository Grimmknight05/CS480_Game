using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DiscoButtonInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private InteractionPromptChannelSO promptChannel;
    [SerializeField] private string promptMessage = "Press E for Disco";
    [SerializeField] private bool forceTriggerCollider = true;
    [SerializeField] private bool autoCreateTriggerCollider = true;
    [SerializeField] private float triggerRadius = 2.2f;
    [SerializeField] private Vector3 triggerCenter = new Vector3(0f, 0.6f, 0f);

    [Header("Interactable Icon")]
    [SerializeField] private bool autoCreateHolographicIcon = true;
    [SerializeField] private HolographicInteractableIcon holographicIcon;
    [SerializeField] private float holographicIconScaleMultiplier = 4.5706077f;

    [Header("Button Press")]
    [SerializeField] private Transform capTransform;
    [SerializeField] private string capName = "Cap";
    [SerializeField] private Vector3 pressedLocalOffset = new Vector3(0f, -0.08f, 0f);

    [Header("Disco Effect")]
    [SerializeField] private float discoDuration = 5f;
    [SerializeField] private float flickerInterval = 0.08f;
    [SerializeField] private int autoLightCount = 8;
    [SerializeField] private float autoLightRadius = 0.5f;
    [SerializeField] private float autoLightHeight = 0f;
    [SerializeField] private float lightRange = 32f;
    [SerializeField] private float lightIntensity = 22f;
    [SerializeField] private float roomFillLightRange = 44f;
    [SerializeField] private float roomFillLightIntensity = 30f;
    [SerializeField] private float colorSaturation = 1f;
    [SerializeField] private float colorValue = 1f;

    [Header("Disco Ball")]
    [SerializeField] private Transform discoBall;
    [SerializeField] private bool autoFindDiscoBall = true;
    [SerializeField] private string discoBallName = "disco";
    [SerializeField] private Vector3 discoBallLoweredLocalOffset = new Vector3(0f, -4.5f, 0f);
    [SerializeField] private float discoBallMoveSpeed = 4.5f;
    [SerializeField] private float discoBallSpinSpeed = 115f;
    [SerializeField] private Vector3 discoBallSpinAxis = Vector3.up;
    [SerializeField] private float discoBallEmissionIntensity = 4f;

    private bool playerInRange;
    private bool promptShown;
    private bool isDiscoRunning;
    private Vector3 capRestLocalPosition;
    private bool discoBallLowered;
    private bool hasDiscoBallRestPose;
    private Vector3 discoBallRestLocalPosition;
    private Renderer[] capRenderers;
    private Renderer[] discoBallRenderers;
    private Light[] discoLights;
    private Light roomFillLight;
    private Coroutine discoCoroutine;
    private MaterialPropertyBlock propertyBlock;
    private MaterialPropertyBlock discoBallPropertyBlock;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Reset()
    {
        ConfigureTriggerCollider();
        ResolveCap();
    }

    private void Awake()
    {
        ConfigureTriggerCollider();
        ResolveCap();
        CacheCapRenderers();
        ResolveDiscoBall();
        ResolveHolographicIcon();
        RefreshHolographicIcon();
    }

    private void Update()
    {
        UpdateDiscoBallMotion();
    }

    private void OnEnable()
    {
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
        RefreshHolographicIcon();
    }

    private void OnDisable()
    {
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
        HidePrompt();
        StopDisco();

        if (holographicIcon != null)
            holographicIcon.SetHighlighted(false);
    }

    private void HandleInteractPressed()
    {
        if (!playerInRange)
            return;

        StartDisco();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (!isDiscoRunning)
            ShowPrompt();

        RefreshHolographicIcon();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HidePrompt();
        RefreshHolographicIcon();
    }

    private void StartDisco()
    {
        if (discoCoroutine != null)
        {
            StopCoroutine(discoCoroutine);
            discoCoroutine = null;
        }

        ResetDiscoState();
        discoCoroutine = StartCoroutine(RunDisco());
    }

    private IEnumerator RunDisco()
    {
        isDiscoRunning = true;
        HidePrompt();
        SetCapPressed(true);
        EnsureDiscoLights();
        SetDiscoBallLowered(true);

        float endTime = Time.time + Mathf.Max(0.1f, discoDuration);
        WaitForSeconds delay = new WaitForSeconds(Mathf.Max(0.02f, flickerInterval));

        while (Time.time < endTime)
        {
            FlickerRainbow();
            yield return delay;
        }

        discoCoroutine = null;
        ResetDiscoState();

        if (playerInRange)
            ShowPrompt();

        RefreshHolographicIcon();
    }

    private void StopDisco()
    {
        if (discoCoroutine != null)
        {
            StopCoroutine(discoCoroutine);
            discoCoroutine = null;
        }

        ResetDiscoState();
    }

    private void ResetDiscoState()
    {
        isDiscoRunning = false;
        SetCapPressed(false);
        ClearCapEmission();
        SetDiscoBallLowered(false);
        ClearDiscoBallEmission();

        if (roomFillLight != null)
            roomFillLight.enabled = false;

        if (discoLights == null)
            return;

        foreach (Light discoLight in discoLights)
        {
            if (discoLight == null)
                continue;

            discoLight.enabled = false;
        }
    }

    private void FlickerRainbow()
    {
        EnsureDiscoLights();

        for (int index = 0; index < discoLights.Length; index++)
        {
            Light discoLight = discoLights[index];
            if (discoLight == null)
                continue;

            Color color = RandomRainbowColor(index);
            discoLight.color = color;
            discoLight.intensity = Random.Range(lightIntensity * 0.55f, lightIntensity);
            discoLight.range = lightRange;
            discoLight.enabled = true;

            if (index == 0)
            {
                ApplyCapEmission(color);
                ApplyDiscoBallEmission(color);
                ApplyRoomFillLight(color);
            }
        }
    }

    private Color RandomRainbowColor(int index)
    {
        float hue = Mathf.Repeat(Time.time * 0.35f + index * 0.31f + Random.Range(-0.08f, 0.08f), 1f);
        return Color.HSVToRGB(hue, Mathf.Clamp01(colorSaturation), Mathf.Clamp01(colorValue));
    }

    private void SetCapPressed(bool pressed)
    {
        if (capTransform == null)
            return;

        capTransform.localPosition = pressed
            ? capRestLocalPosition + pressedLocalOffset
            : capRestLocalPosition;
    }

    private void ApplyCapEmission(Color color)
    {
        if (capRenderers == null || capRenderers.Length == 0)
            return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        Color emission = color * Mathf.Max(0.1f, lightIntensity * 0.5f);

        foreach (Renderer renderer in capRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                if (materials[materialIndex] != null)
                    materials[materialIndex].EnableKeyword("_EMISSION");

                propertyBlock.Clear();
                renderer.GetPropertyBlock(propertyBlock, materialIndex);
                propertyBlock.SetColor(EmissionColorId, emission);
                renderer.SetPropertyBlock(propertyBlock, materialIndex);
            }
        }
    }

    private void ClearCapEmission()
    {
        if (capRenderers == null)
            return;

        foreach (Renderer renderer in capRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                renderer.SetPropertyBlock(null, materialIndex);
        }
    }

    private void EnsureDiscoLights()
    {
        int lightCount = Mathf.Max(1, autoLightCount);
        ResolveDiscoBall();
        EnsureRoomFillLight();

        if (discoLights != null && discoLights.Length == lightCount)
        {
            for (int index = 0; index < discoLights.Length; index++)
                ConfigureDiscoLight(discoLights[index], index, lightCount);

            return;
        }

        discoLights = new Light[lightCount];
        for (int index = 0; index < lightCount; index++)
        {
            GameObject lightObject = new GameObject($"Disco Rainbow Light {index + 1}");
            Light discoLight = lightObject.AddComponent<Light>();
            discoLights[index] = discoLight;
            ConfigureDiscoLight(discoLight, index, lightCount);
        }
    }

    private void EnsureRoomFillLight()
    {
        if (roomFillLight == null)
        {
            GameObject lightObject = new GameObject("Disco Room Fill Light");
            roomFillLight = lightObject.AddComponent<Light>();
        }

        Transform lightParent = discoBall != null ? discoBall : transform;
        roomFillLight.transform.SetParent(lightParent, false);
        roomFillLight.transform.localPosition = Vector3.zero;
        roomFillLight.type = LightType.Point;
        roomFillLight.range = roomFillLightRange;
        roomFillLight.intensity = roomFillLightIntensity;
        roomFillLight.bounceIntensity = 1.5f;
        roomFillLight.shadows = LightShadows.None;
        roomFillLight.enabled = false;
    }

    private void ConfigureDiscoLight(Light discoLight, int index, int lightCount)
    {
        if (discoLight == null)
            return;

        Transform lightParent = discoBall != null ? discoBall : transform;
        discoLight.transform.SetParent(lightParent, false);

        float angle = index * Mathf.PI * 2f / lightCount;
        discoLight.transform.localPosition = new Vector3(
            Mathf.Cos(angle) * autoLightRadius,
            autoLightHeight,
            Mathf.Sin(angle) * autoLightRadius);

        discoLight.type = LightType.Point;
        discoLight.range = lightRange;
        discoLight.intensity = lightIntensity;
        discoLight.bounceIntensity = 1.5f;
        discoLight.shadows = LightShadows.None;
        discoLight.enabled = false;
    }

    private void ApplyRoomFillLight(Color color)
    {
        EnsureRoomFillLight();

        roomFillLight.color = color;
        roomFillLight.range = roomFillLightRange;
        roomFillLight.intensity = Random.Range(roomFillLightIntensity * 0.8f, roomFillLightIntensity);
        roomFillLight.enabled = true;
    }

    private void ConfigureTriggerCollider()
    {
        Collider trigger = GetComponent<Collider>();

        if (trigger == null && autoCreateTriggerCollider)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = triggerRadius;
            sphere.center = triggerCenter;
            trigger = sphere;
        }
        else if (trigger is SphereCollider sphere)
        {
            sphere.radius = triggerRadius;
            sphere.center = triggerCenter;
        }

        if (trigger != null && forceTriggerCollider)
            trigger.isTrigger = true;
    }

    private void ResolveCap()
    {
        if (capTransform == null)
            capTransform = FindChildByName(transform, capName);

        if (capTransform != null)
            capRestLocalPosition = capTransform.localPosition;
    }

    private void ResolveDiscoBall()
    {
        if (discoBall == null && autoFindDiscoBall)
            discoBall = FindTransformByName(discoBallName);

        if (discoBall == null)
            return;

        CaptureDiscoBallRestPose();
        discoBallRenderers = discoBall.GetComponentsInChildren<Renderer>(true);
    }

    private void CaptureDiscoBallRestPose()
    {
        if (hasDiscoBallRestPose || discoBall == null)
            return;

        discoBallRestLocalPosition = discoBall.localPosition;
        hasDiscoBallRestPose = true;
    }

    private void SetDiscoBallLowered(bool lowered)
    {
        ResolveDiscoBall();
        discoBallLowered = lowered;
    }

    private void UpdateDiscoBallMotion()
    {
        if (discoBall == null)
            return;

        CaptureDiscoBallRestPose();

        Vector3 target = discoBallRestLocalPosition + (discoBallLowered ? discoBallLoweredLocalOffset : Vector3.zero);
        discoBall.localPosition = Vector3.MoveTowards(
            discoBall.localPosition,
            target,
            Mathf.Max(0.1f, discoBallMoveSpeed) * Time.deltaTime);

        if (!discoBallLowered)
            return;

        Vector3 spinAxis = discoBallSpinAxis.sqrMagnitude > 0.001f ? discoBallSpinAxis.normalized : Vector3.up;
        discoBall.Rotate(spinAxis, discoBallSpinSpeed * Time.deltaTime, Space.World);
    }

    private void ApplyDiscoBallEmission(Color color)
    {
        if (discoBallRenderers == null || discoBallRenderers.Length == 0)
            return;

        if (discoBallPropertyBlock == null)
            discoBallPropertyBlock = new MaterialPropertyBlock();

        Color emission = color * Mathf.Max(0f, discoBallEmissionIntensity);

        foreach (Renderer renderer in discoBallRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                if (materials[materialIndex] != null)
                    materials[materialIndex].EnableKeyword("_EMISSION");

                discoBallPropertyBlock.Clear();
                renderer.GetPropertyBlock(discoBallPropertyBlock, materialIndex);
                discoBallPropertyBlock.SetColor(EmissionColorId, emission);
                renderer.SetPropertyBlock(discoBallPropertyBlock, materialIndex);
            }
        }
    }

    private void ClearDiscoBallEmission()
    {
        if (discoBallRenderers == null)
            return;

        foreach (Renderer renderer in discoBallRenderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                renderer.SetPropertyBlock(null, materialIndex);
        }
    }

    private void CacheCapRenderers()
    {
        if (capTransform != null)
            capRenderers = capTransform.GetComponentsInChildren<Renderer>(true);
        else
            capRenderers = GetComponentsInChildren<Renderer>(true);
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        foreach (Transform child in root)
        {
            Transform match = FindChildByName(child, childName);
            if (match != null)
                return match;
        }

        return null;
    }

    private Transform FindTransformByName(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            return null;

        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform sceneTransform in sceneTransforms)
        {
            if (sceneTransform != null && sceneTransform.name == targetName)
                return sceneTransform;
        }

        return null;
    }

    private void ResolveHolographicIcon()
    {
        if (!autoCreateHolographicIcon)
            return;

        if (holographicIcon == null)
            holographicIcon = GetComponentInChildren<HolographicInteractableIcon>(true);

        if (holographicIcon == null)
            holographicIcon = gameObject.AddComponent<HolographicInteractableIcon>();

        holographicIcon.SetScaleMultiplier(holographicIconScaleMultiplier);
    }

    private void RefreshHolographicIcon()
    {
        ResolveHolographicIcon();

        if (holographicIcon == null)
            return;

        holographicIcon.SetVisible(true);
        holographicIcon.SetHighlighted(playerInRange && !isDiscoRunning);
    }

    private void ShowPrompt()
    {
        if (promptChannel == null || promptShown)
            return;

        promptChannel.Raise(new InteractionPromptData(this, true, promptMessage));
        promptShown = true;
    }

    private void HidePrompt()
    {
        if (promptChannel == null || !promptShown)
            return;

        promptChannel.Raise(new InteractionPromptData(this, false, string.Empty));
        promptShown = false;
    }
}
