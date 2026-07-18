using BetterAmongUs.Attributes;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace BetterAmongUs.MonoScripts;

/// <summary>
/// Animates a map icon by smoothly scaling it up and down when conditions are met.
/// </summary>
[RegisterInIl2Cpp]
internal class AnimatedMapIcon : MonoBehaviour
{
    /// <summary>
    /// Event that determines whether the icon should animate.
    /// Returns true if animation should occur, false otherwise.
    /// </summary>
    [HideFromIl2Cpp]
    internal event Func<bool>? ShouldAnimate;

    private Vector3 originalScale;

    /// <summary>
    /// The multiplier applied to the original scale when fully animated.
    /// </summary>
    private readonly float scaleMultiplier = 1.2f;

    /// <summary>
    /// The speed of the animation loop.
    /// </summary>
    private readonly float animationSpeed = 2f;

    /// <summary>
    /// Initializes the component by storing the original scale of the icon.
    /// </summary>
    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Updates the icon scale each frame based on the animation state.
    /// When ShouldAnimate returns true, creates a smooth ping-pong scaling effect.
    /// When ShouldAnimate returns false, resets to the original scale.
    /// </summary>
    private void Update()
    {
        if (ShouldAnimate?.Invoke() == true)
        {
            float pingPong = Mathf.PingPong(Time.time * animationSpeed, 1f);
            float t = Mathf.SmoothStep(0f, 1f, pingPong);
            Vector3 targetScale = originalScale * (1f + (scaleMultiplier - 1f) * t);

            transform.localScale = targetScale;
        }
        else
        {
            transform.localScale = originalScale;
        }
    }
}