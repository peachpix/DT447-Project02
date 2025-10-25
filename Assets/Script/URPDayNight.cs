using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPDayNight : MonoBehaviour
{
    [Header("Time (Durations in seconds)")]
    public bool autoRun = true;
    public bool startAtSunrise = true;
    public float sunriseDur = 25f;
    public float dayDur = 80f;
    public float sunsetDur = 25f;
    public float nightDur = 60f;

    [Tooltip("Manual time scrub (0..1 across full loop). Ignored if autoRun is true.")]
    [Range(0f, 1f)] public float timeOfDay = 0f;

    [Header("Sun & Moon")]
    public Light sun;
    public Light moon;
    public Vector3 sunAxis = new Vector3(1, 0, 0);
    public Gradient sunColorOverDay;
    public AnimationCurve sunIntensityOverDay;

    [Header("Skyboxes (hard swap per phase)")]
    public Material skyboxSunrise;
    public Material skyboxDay;
    public Material skyboxSunset;
    public Material skyboxNight;

    [Header("Environment")]
    public bool manageFog = true;
    public Gradient fogColorOverDay;
    public AnimationCurve fogDensityOverDay;

    [Header("Night Darkness (URP Volume)")]
    public Volume globalVolume;
    public float dayPostExposure = 0f;
    public float nightPostExposure = -2.5f;
    public float nightFogBoost = 1.6f;
    [Range(0f, 1f)] public float nightAmbientMultiplier = 0.2f;

    [Header("Debug")]
    public bool debugPhase = false;

    // internals
    float elapsed, totalDur;
    // normalized boundaries (0..1)
    float t1_sunriseEnd, t2_dayEnd, t3_sunsetEnd;
    Material lastAppliedSky;
    bool startedFromAwake;

    enum Phase { Sunrise, Day, Sunset, Night }

    void Reset()
    {
        sunIntensityOverDay = new AnimationCurve(
            new Keyframe(0.00f, 0.00f),
            new Keyframe(0.20f, 0.55f),
            new Keyframe(0.25f, 0.90f),
            new Keyframe(0.50f, 1.10f),
            new Keyframe(0.75f, 0.90f),
            new Keyframe(0.80f, 0.55f),
            new Keyframe(1.00f, 0.00f)
        );
        fogDensityOverDay = AnimationCurve.Linear(0, 0.01f, 1, 0.01f);
    }

    void Awake()
    {
        RecalcTimeline();

        if (startAtSunrise)
        {
            // Start at the middle of the sunrise block (guaranteed Sunrise)
            float tStart = t1_sunriseEnd * 0.4f; // 40% into sunrise
            SetTime01(tStart);
            lastAppliedSky = null;
            ApplyCycle(true);
            startedFromAwake = true; // avoid advancing on the very first Update
        }
    }

    void Start()
    {
        if (!startAtSunrise) ApplyCycle(true);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) { autoRun = false; SetTime01(t1_sunriseEnd * 0.5f); ApplyCycle(true); }                                  // sunrise
        if (Input.GetKeyDown(KeyCode.F2)) { autoRun = false; SetTime01(Mathf.Lerp(t1_sunriseEnd, t2_dayEnd, 0.5f)); ApplyCycle(true); }          // day
        if (Input.GetKeyDown(KeyCode.F3)) { autoRun = false; SetTime01(Mathf.Lerp(t2_dayEnd, t3_sunsetEnd, 0.5f)); ApplyCycle(true); }        // sunset
        if (Input.GetKeyDown(KeyCode.F4)) { autoRun = false; SetTime01(Mathf.Lerp(t3_sunsetEnd, 1f, 0.5f)); ApplyCycle(true); }        // night
#endif

        if (autoRun)
        {
            if (startedFromAwake) { startedFromAwake = false; }
            else
            {
                if (totalDur <= 0f) RecalcTimeline();
                elapsed += Time.deltaTime;
                if (elapsed > totalDur) elapsed -= totalDur;
                timeOfDay = Mathf.Clamp01(elapsed / totalDur);
            }
        }

        ApplyCycle(false);
    }

    // ---- timeline (strict ordered boundaries) ----
    void RecalcTimeline()
    {
        totalDur = Mathf.Max(0.01f, sunriseDur + dayDur + sunsetDur + nightDur);

        // normalized ends: [0..t1) Sunrise, [t1..t2) Day, [t2..t3) Sunset, [t3..1) Night
        t1_sunriseEnd = sunriseDur / totalDur;
        t2_dayEnd = (sunriseDur + dayDur) / totalDur;
        t3_sunsetEnd = (sunriseDur + dayDur + sunsetDur) / totalDur;

        elapsed = timeOfDay * totalDur;
    }

    void SetTime01(float t01)
    {
        timeOfDay = Mathf.Repeat(t01, 1f);
        elapsed = timeOfDay * totalDur;
    }

    Phase GetPhaseOrdered(float t)
    {
        if (t < t1_sunriseEnd) return Phase.Sunrise;
        else if (t < t2_dayEnd) return Phase.Day;
        else if (t < t3_sunsetEnd) return Phase.Sunset;
        else return Phase.Night;
    }

    void ApplyCycle(bool force)
    {
        // 1) rotate sun & moon
        float sunAngle = timeOfDay * 360f;
        var rot = Quaternion.AngleAxis(sunAngle - 90f, sunAxis.normalized);
        if (sun) sun.transform.rotation = rot;
        if (moon) moon.transform.rotation = rot * Quaternion.Euler(180f, 0, 0);

        // 2) strict phase (sunrise -> day -> sunset -> night)
        Phase phase = GetPhaseOrdered(timeOfDay);

        // 3) HARD skybox per phase
        Material target = skyboxNight;
        if (phase == Phase.Sunrise) target = skyboxSunrise;
        else if (phase == Phase.Day) target = skyboxDay;
        else if (phase == Phase.Sunset) target = skyboxSunset;

        if (force || lastAppliedSky != target)
        {
            RenderSettings.skybox = target;
            //if (debugPhase) Debug.Log("Skybox → " + (target ? target.name : "NULL"));
            lastAppliedSky = target;
        }

        

        // 4) sun / moon color & intensity (your curves/gradients)
        if (sunColorOverDay != null) sun.color = sunColorOverDay.Evaluate(timeOfDay);
        if (sunIntensityOverDay != null) sun.intensity = sunIntensityOverDay.Evaluate(timeOfDay);
        if (sun) sun.shadows = LightShadows.Soft;

        if (moon)
        {
            // show moon only in the Night block
            bool nightNow = (phase == Phase.Night);
            moon.enabled = nightNow;
            if (nightNow) moon.intensity = 0.08f;
        }

        // 5) fog
        if (manageFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            if (fogColorOverDay != null) RenderSettings.fogColor = fogColorOverDay.Evaluate(timeOfDay);
            float baseFog = fogDensityOverDay != null ? fogDensityOverDay.Evaluate(timeOfDay) : RenderSettings.fogDensity;

            // little extra fog only during Night block
            float nightBoost = (phase == Phase.Night) ? nightFogBoost : 1f;
            RenderSettings.fogDensity = baseFog * nightBoost;
        }

        // 6) URP Volume exposure (dark only during Night block)
        if (globalVolume != null && globalVolume.profile != null &&
            globalVolume.profile.TryGet<ColorAdjustments>(out var colorAdj))
        {
            colorAdj.postExposure.value = (phase == Phase.Night) ? nightPostExposure : dayPostExposure;
        }

        // 7) Ambient (dark only during Night block)
        RenderSettings.ambientIntensity = (phase == Phase.Night) ? nightAmbientMultiplier : 1f;
    }

    void OnValidate() { RecalcTimeline(); }
}
