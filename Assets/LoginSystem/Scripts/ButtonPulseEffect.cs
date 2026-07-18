using System.Collections;
using UnityEngine;

/// <summary>
/// Attach directly to a UI Button's GameObject to make it continuously
/// pulse between a "zoomed out" and "zoomed in" scale — useful for drawing
/// attention to a call-to-action like a Sign In button.
/// </summary>
[DisallowMultipleComponent]
public class ButtonPulseEffect : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("Scale multiplier at the smallest point of the pulse (1 = original size)")]
    [SerializeField] private float zoomOutScale = 1f;
    [Tooltip("Scale multiplier at the largest point of the pulse (1.15 = 15% bigger)")]
    [SerializeField] private float zoomInScale = 1.15f;

    [Header("Timing")]
    [Tooltip("Seconds to go from zoomed-out to zoomed-in (and the same again on the way back)")]
    [SerializeField] private float duration = 0.6f;
    [Tooltip("Ease shape of the pulse, evaluated 0→1 over 'duration'")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Behaviour")]
    [Tooltip("Start pulsing automatically as soon as this GameObject becomes active")]
    [SerializeField] private bool playOnEnable = true;
    [Tooltip("Keep pulsing even if the game is paused (Time.timeScale = 0)")]
    [SerializeField] private bool useUnscaledTime = false;

    private Vector3 baseScale;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (playOnEnable)
            StartPulsing();
    }

    private void OnDisable()
    {
        StopPulsing();
        transform.localScale = baseScale; // reset so it doesn't get stuck mid-pulse
    }

    public void StartPulsing()
    {
        StopPulsing();
        pulseRoutine = StartCoroutine(PulseLoop());
    }

    public void StopPulsing()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return ScaleOverTime(zoomOutScale, zoomInScale, duration);
            yield return ScaleOverTime(zoomInScale, zoomOutScale, duration);
        }
    }

    private IEnumerator ScaleOverTime(float from, float to, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            float eased = easeCurve.Evaluate(t);
            float factor = Mathf.Lerp(from, to, eased);
            transform.localScale = baseScale * factor;
            yield return null;
        }

        transform.localScale = baseScale * to;
    }
}