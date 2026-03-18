using BetterAmongUs.Modules.Support;
using UnityEngine;

namespace BetterAmongUs.Helpers;

/// <summary>
/// Provides helper methods for working with Unity GameObjects, components, and visual elements.
/// </summary>
internal static class ObjectHelper
{
    /// <summary>
    /// Destroys a GameObject if it is not null.
    /// </summary>
    /// <param name="obj">The GameObject to destroy.</param>
    internal static void DestroyObj(this GameObject obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Destroys the GameObject associated with a MonoBehaviour if it is not null.
    /// </summary>
    /// <param name="mono">The MonoBehaviour whose GameObject should be destroyed.</param>
    internal static void DestroyObj(this MonoBehaviour mono) => mono?.gameObject?.DestroyObj();

    /// <summary>
    /// Destroys a MonoBehaviour component if it is not null.
    /// </summary>
    /// <param name="mono">The MonoBehaviour to destroy.</param>
    internal static void DestroyMono(this MonoBehaviour mono) => UnityEngine.Object.Destroy(mono);

    /// <summary>
    /// Destroys all TextTranslatorTMP components in the children of a GameObject.
    /// </summary>
    /// <param name="obj">The GameObject to search for TextTranslatorTMP components.</param>
    internal static void DestroyTextTranslators(this GameObject obj)
    {
        var translators = obj.GetComponentsInChildren<TextTranslatorTMP>();
        if (translators.Length > 0)
        {
            foreach (var item in translators)
            {
                item.DestroyMono();
            }
        }
    }

    /// <summary>
    /// Destroys all TextTranslatorTMP components in the children of a MonoBehaviour's GameObject.
    /// </summary>
    /// <param name="mono">The MonoBehaviour whose GameObject should be searched.</param>
    internal static void DestroyTextTranslators(this MonoBehaviour mono) => mono.gameObject.DestroyTextTranslators();


    /// <summary>
    /// Destroys a specified component on a game object 
    /// </summary>
    /// <typeparam name="T">The Type of the Component to destroy</typeparam>
    /// <param name="obj"></param>
    internal static void RemoveComponent<T>(this GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (comp != null)
        {
            UnityEngine.Object.Destroy(comp);
        }
    }

    /// <summary>
    /// Sets the color of all SpriteRenderer components in the children of a GameObject.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="color">The color to apply to all sprites.</param>
    internal static void SetSpriteColors(this GameObject go, Color color)
    {
        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            sprite.color = color;
        }
    }

    /// <summary>
    /// Applies a custom action to all SpriteRenderer components in the children of a GameObject.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="setSprite">The action to apply to each SpriteRenderer.</param>
    internal static void SetSpriteColors(this GameObject go, Action<SpriteRenderer> setSprite)
    {
        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            setSprite(sprite);
        }
    }

    /// <summary>
    /// Sets UI colors for all SpriteRenderer components while avoiding specific GameObject names.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="avoidGoName">Names of GameObjects to skip when applying colors.</param>
    internal static void SetUIColors(this GameObject go, params string[] avoidGoName)
    {
        go.SetUIColors(null, null, avoidGoName);
    }

    /// <summary>
    /// Sets UI colors for SpriteRenderer components with an optional check function.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="check">Optional function to determine if a SpriteRenderer should be modified.</param>
    /// <param name="avoidGoName">Names of GameObjects to skip when applying colors.</param>
    internal static void SetUIColors(this GameObject go, Func<SpriteRenderer, bool>? check = null, params string[] avoidGoName)
    {
        go.SetUIColors(null, check, avoidGoName);
    }

    /// <summary>
    /// Sets UI colors for SpriteRenderer components with specific color and filtering options.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="color">The color to apply (defaults to green if null).</param>
    /// <param name="check">Optional function to determine if a SpriteRenderer should be modified.</param>
    /// <param name="avoidGoName">Names of GameObjects to skip when applying colors.</param>
    internal static void SetUIColors(this GameObject go, Color? color = null, Func<SpriteRenderer, bool>? check = null, params string[] avoidGoName)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Theme))
            return;

        var sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            if (avoidGoName.Any(name => sprite.gameObject.name == name)) continue;
            if (check == null || check(sprite))
                AddColor(sprite, color);
        }
    }

    /// <summary>
    /// Adds a color tint to a SpriteRenderer using a blend formula.
    /// </summary>
    /// <param name="sprite">The SpriteRenderer to modify.</param>
    /// <param name="color">The color to blend (defaults to green if null).</param>
    internal static void AddColor(SpriteRenderer sprite, Color? color = null)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Theme))
            return;

        if (color == null)
        {
            color = Color.green;
        }

        sprite.color = (sprite.color * 0.6f) + ((Color)color * 0.5f);
    }

    /// <summary>
    /// Sets the layer for a GameObject and all its children.
    /// </summary>
    /// <param name="go">The GameObject to modify.</param>
    /// <param name="layerName">The name of the layer to set.</param>
    internal static void SetLayers(this GameObject go, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
            return;

        Transform[] allChildren = go.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = layer;
        }
    }
}