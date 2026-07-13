using BetterAmongUs.Attributes;
using BetterAmongUs.Interfaces;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.MonoScripts.Extended;

/// <summary>
/// Extends PassiveButton with hold and shift-click functionality.
/// </summary>
[RegisterInIl2Cpp]
internal class ExtendedPassiveButton : MonoBehaviour, IMonoExtension<PassiveButton>
{
    /// <summary>
    /// Gets or sets the base PassiveButton instance.
    /// </summary>
    public PassiveButton? BaseMono { get; set; }

    /// <summary>
    /// Triggered when the button has been held down for the duration specified by <see cref="HoldTriggerTime"/>.
    /// </summary>
    public Action? OnHold = null;

    /// <summary>
    /// Triggered either when the button is held (after hold trigger time) OR when shift-clicking.
    /// </summary>
    public Action? OnHoldOrShiftClick = null;

    /// <summary>
    /// Triggered when shift-clicking the button (LeftShift + click).
    /// </summary>
    public Action? OnShiftClick = null;

    /// <summary>
    /// Time in seconds the button must be held to trigger hold events.
    /// </summary>
    public float HoldTriggerTime = 0.5f;

    private bool m_isHolding;
    private float m_holdTimer;
    private bool m_suppressClick;

    public void OnExtensionAwake(PassiveButton passiveButton)
    {
        passiveButton.OnDown = true;
    }

    public void Update()
    {
        if (m_isHolding)
        {
            m_holdTimer += Time.deltaTime;
            if (m_holdTimer >= HoldTriggerTime)
            {
                BaseMono.ReceiveClickUp();
            }
        }
    }

    public void OnDestroy()
    {
        IMonoExtension.TryRemoveExtension(this);
    }

    /// <summary>
    /// Called when the button is pressed down to start tracking hold state.
    /// </summary>
    public void TriggerClickDown()
    {
        m_isHolding = true;
        m_suppressClick = false;
        m_holdTimer = 0f;
    }

    /// <summary>
    /// Called when the button is released. Determines whether to trigger hold end, shift-click, or the base click event.
    /// </summary>
    public void TriggerClickUp()
    {
        if (m_holdTimer >= HoldTriggerTime)
        {
            OnHold?.Invoke();
            OnHoldOrShiftClick?.Invoke();
            m_suppressClick = true;
        }
        else
        {
            if (m_suppressClick)
            {
                m_suppressClick = false;
                return;
            }

            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && (OnShiftClick != null || OnHoldOrShiftClick != null))
            {
                OnShiftClick?.Invoke();
                OnHoldOrShiftClick?.Invoke();
            }
            else if (BaseMono.OnClick != null)
            {
                BaseMono.OnClick.Invoke();
                SoundManager.instance.PlaySound(BaseMono.ClickSound, false, 1);
            }
        }

        m_isHolding = false;
        m_holdTimer = 0f;
    }

    [HarmonyPatch]
    private static class ExtendedPassiveButtonPatch
    {
        [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
        [HarmonyPrefix]
        private static bool PassiveButton_ReceiveClickDown_Prefix(PassiveButton __instance)
        {
            var ext = IMonoExtension.GetExtension<ExtendedPassiveButton>(__instance);
            if (ext != null)
            {
                ext.TriggerClickDown();
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
        [HarmonyPrefix]
        private static bool PassiveButton_ReceiveClickUp_Prefix(PassiveButton __instance)
        {
            var ext = IMonoExtension.GetExtension<ExtendedPassiveButton>(__instance);
            if (ext != null)
            {
                ext.TriggerClickUp();
                return false;
            }

            return true;
        }
    }
}