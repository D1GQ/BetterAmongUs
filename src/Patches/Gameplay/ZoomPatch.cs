using AmongUs.GameOptions;
using BetterAmongUs.Utilities;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay;

[HarmonyPatch]
internal class ZoomPatch
{
    private static bool _wasZooming = false; // Track if user was zooming
    private static float _lastOrthographicSize = 0f; // Track last camera size

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    private static void HudManager_Update_Postfix(HudManager __instance)
    {
        // Determine if zooming is allowed:
        // - Player can move AND
        // - Not Guardian Angel AND
        // - Either not in gameplay OR player is dead
        bool canZoom = GameState.IsCanMove &&
              !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel) &&
              (!GameState.IsInGamePlay || !PlayerControl.LocalPlayer.IsAlive());

        // Should we reset zoom (when player was zooming but no longer can)
        bool shouldReset = !canZoom && _wasZooming;

        _lastOrthographicSize = Camera.main.orthographicSize;

        if (shouldReset)
        {
            // Reset to default zoom
            SetZoomSize(reset: true);
            _wasZooming = false;
        }
        else if (canZoom)
        {
            // Handle scroll wheel input
            if (Input.mouseScrollDelta.y > 0 && Camera.main.orthographicSize > 3.0f)
            {
                _wasZooming = true;
                SetZoomSize(zoomIn: true); // Scroll up = zoom in
            }
            else if (Input.mouseScrollDelta.y < 0 &&
                    (GameState.IsDead || GameState.IsFreePlay || GameState.IsLobby) &&
                    Camera.main.orthographicSize < 18.0f)
            {
                _wasZooming = true;
                SetZoomSize(zoomOut: true); // Scroll down = zoom out
            }
        }

        // Show/hide shadow quad based on zoom level and if player is alive
        if (__instance.ShadowQuad != null)
        {
            __instance.ShadowQuad.gameObject.SetActive(Camera.main.orthographicSize == 3.0f && PlayerControl.LocalPlayer.IsAlive());
        }
    }

    // Handles zoom in/out and reset functionality
    private static void SetZoomSize(bool zoomIn = false, bool zoomOut = false, bool reset = false)
    {
        if (!HudManager.InstanceExists) return;

        if (reset)
        {
            // Reset to default zoom (3.0)
            Camera.main.orthographicSize = 3.0f;
            HudManager.Instance.UICamera.orthographicSize = 3.0f;
            HudManager.Instance.Chat.transform.localScale = Vector3.one;

            if (GameState.IsMeeting)
                MeetingHud.Instance.transform.localScale = Vector3.one;
        }
        else if (zoomIn || zoomOut)
        {
            // Zoom in (multiply by 1/1.5 = 0.666) or zoom out (multiply by 1.5)
            float size = zoomIn ? 1 / 1.5f : 1.5f;
            Camera.main.orthographicSize *= size;
            HudManager.Instance.UICamera.orthographicSize *= size;
        }

        // Trigger resolution change event if camera size changed
        if (Camera.main.orthographicSize != _lastOrthographicSize)
        {
            ResolutionManager.ResolutionChanged.Invoke(
                (float)Screen.width / Screen.height,
                Screen.width, Screen.height,
                Screen.fullScreen);
        }
    }
}