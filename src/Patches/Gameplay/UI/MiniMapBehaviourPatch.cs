using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Mono;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class MiniMapBehaviourPatch
{
    // Layer offsets for different icon types (prevents overlapping)
    private const float VentLayerOffset = -0.1f;
    private const float VentArrowLayerOffset = -0.1f; // VentLayerOffset + VentArrowLayerOffset = -0.2
    private const float UsableLayerOffset = -0.3f;

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    [HarmonyPostfix]
    private static void MapBehaviour_ShowNormalMap_Postfix(MapBehaviour __instance)
        => __instance.ColorControl.SetColor(new Color(0.05f, 0.6f, 1f, 1f)); // Blue tint for normal map

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowDetectiveMap))]
    [HarmonyPostfix]
    private static void MapBehaviour_ShowDetectiveMap_Postfix(MapBehaviour __instance)
        => __instance.ColorControl.SetColor(new Color(0.05f, 0.6f, 1f, 1f)); // Blue tint for detective map

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    [HarmonyPostfix]
    private static void MapBehaviour_ShowSabotageMap_Postfix(MapBehaviour __instance)
        => __instance.ColorControl.SetColor(new Color(1f, 0.3f, 0f, 1f)); // Orange tint for sabotage map

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    [HarmonyPostfix]
    private static void MapCountOverlay_OnEnable_Postfix(MapCountOverlay __instance)
        // Green background normally, gray if comms sabotaged
        => __instance.BackgroundColor.SetColor(PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer) ? Palette.DisabledGrey : new Color(0.2f, 0.5f, 0f, 1f));

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    [HarmonyPrefix]
    private static void MapCountOverlay_Update_Prefix(MapCountOverlay __instance)
    {
        // Handle comms sabotage effect on map
        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
        {
            // Comms sabotaged - disable map
            __instance.isSab = true;
            __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
            __instance.SabotageText.gameObject.SetActive(true);
        }
        else
        {
            // Comms working - normal map
            __instance.isSab = false;
            __instance.BackgroundColor.SetColor(new Color(0.2f, 0.5f, 0f, 1f));
            __instance.SabotageText.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    [HarmonyPostfix]
    private static void MapConsole_ShowCountOverlay_Postfix()
        => MapBehaviour.Instance.ColorControl.SetColor(new Color(0.2f, 0.5f, 0f, 1f)); // Green tint for admin map

    private static Transform? _icons; // Container for all custom map icons

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
    [HarmonyPostfix]
    private static void MapBehaviour_Show_Postfix(MapBehaviour __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_MinimapIcons))
            return;

        if (!BAUConfigs.MinimapIcons.Value)
            return;

        // Make infected overlay buttons semi-transparent and smaller
        foreach (var button in __instance.infectedOverlay.allButtons)
        {
            button.spriteRenderer.color = new Color(1f, 1f, 1f, 0.8f);
            button.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }

        // Create icons if they don't exist yet
        if (_icons == null)
        {
            // Create container for all icons
            var icons = new GameObject("Icons")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            icons.transform.SetParent(__instance.transform.Find("HereIndicatorParent"));
            icons.transform.localPosition = Vector3.zero;
            icons.transform.localScale = Vector3.one;
            _icons = icons.transform;

            // Add vent icons
            foreach (var vent in BAUPlugin.AllVents)
            {
                CreateVentIcon(vent);
            }

            // Add map console icons
            foreach (var map in UnityEngine.Object.FindObjectsOfType<MapConsole>())
            {
                CreateUsableIcon(map.Cast<IUsable>());
            }

            // Add system console icons
            foreach (var system in UnityEngine.Object.FindObjectsOfType<SystemConsole>())
            {
                CreateSystemConsoleIcon(system);
            }

            // Add zipline icons
            foreach (var zipline in UnityEngine.Object.FindObjectsOfType<ZiplineConsole>())
            {
                var icon = CreateIcon(zipline.image.sprite, "SystemIcon");
                icon.color = Color.white * 0.7f;
                icon.transform.localScale = Vector3.one * 0.25f;
                SetPosFromShip(zipline.transform.position, icon.transform, new Vector3(0f, 0.16f, UsableLayerOffset));
            }

            // Add task console icons for critical sabotages
            foreach (var console in UnityEngine.Object.FindObjectsOfType<Console>())
            {
                foreach (var taskType in console.TaskTypes)
                {
                    // Check if this console is for critical sabotage tasks
                    if (taskType is TaskTypes.FixLights or TaskTypes.FixComms or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic)
                    {
                        var icon = CreateIcon(console.Image.sprite, "ConsoleIcon");
                        icon.color = Color.white * 0.7f;
                        icon.transform.localScale = Vector3.one * 0.5f;
                        SetPosFromShip(console.transform.position, icon.transform, new Vector3(0f, 0f, UsableLayerOffset));

                        // Add animation when player has this task
                        var animatedMapIcon = icon.gameObject.AddComponent<AnimatedMapIcon>();
                        animatedMapIcon.ShouldAnimate += () =>
                        {
                            return console.FindTask(PlayerControl.LocalPlayer) != null;
                        };
                        break;
                    }
                }
            }
        }
    }

    // Creates a vent icon with connection arrows to neighboring vents
    private static void CreateVentIcon(Vent vent)
    {
        var icon = CreateIcon(Utils.LoadSprite("BetterAmongUs.Resources.Images.Icons.Vent.png", 380), "VentIcon");
        var color = VentGroups.GetVentGroupColor(vent);
        icon.color = new Color(color.r, color.g, color.b, 0.7f);

        SetPosFromShip(vent.transform.position, icon.transform, new Vector3(0f, 0f, VentLayerOffset));

        // Get connected vents
        Vent[] nearbyVents = [vent.Left, vent.Right, vent.Center];
        float maxSpreadShift = 0.5f;
        float minSpreadShift = 0.2f;
        float closeDistance = 10f;

        // Create arrows to each connected vent
        for (int i = 0; i < nearbyVents.Length; i++)
        {
            Vent neighborVent = nearbyVents[i];
            if (neighborVent)
            {
                var arrowIcon = CreateIcon(Utils.LoadSprite("BetterAmongUs.Resources.Images.Icons.Arrow.png", 600), "VentArrowIcon");
                arrowIcon.color = VentGroups.GetVentGroupColor(neighborVent);
                arrowIcon.transform.SetParent(icon.transform);

                // Calculate arrow position and rotation based on neighbor location
                Vector3 directionToNeighbor = neighborVent.transform.position - vent.transform.position;
                float distanceToNeighbor = directionToNeighbor.magnitude;

                // Adjust arrow spread based on distance
                float spreadShift;
                if (distanceToNeighbor < closeDistance)
                {
                    float t = distanceToNeighbor / closeDistance;
                    spreadShift = Mathf.Lerp(minSpreadShift, maxSpreadShift, t);
                }
                else
                {
                    spreadShift = maxSpreadShift;
                }

                Vector3 arrowOffset = directionToNeighbor.normalized * (0.7f + spreadShift);
                arrowOffset.y -= 0.08f;

                SetPosFromShip(arrowOffset, arrowIcon.transform, new Vector3(0f, 0f, VentArrowLayerOffset));

                // Rotate arrow to point toward neighbor
                Vector3 ventMapPos = vent.transform.position / ShipStatus.Instance.MapScale;
                ventMapPos.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);

                Vector3 neighborMapPos = neighborVent.transform.position / ShipStatus.Instance.MapScale;
                neighborMapPos.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);

                Vector3 mapDirection = neighborMapPos - ventMapPos;

                float angle = Mathf.Atan2(mapDirection.y, mapDirection.x) * Mathf.Rad2Deg;
                arrowIcon.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
    }

    // Creates icon for system consoles
    private static void CreateSystemConsoleIcon(SystemConsole systemConsole)
    {
        if (systemConsole.UseIcon is not ImageNames.UseButton)
        {
            CreateUsableIcon(systemConsole.Cast<IUsable>());
        }
        else
        {
            var icon = CreateIcon(systemConsole.Image.sprite, "SystemIcon");
            icon.color = Color.white * 0.7f;
            icon.transform.localScale = Vector3.one * 0.4f;
            SetPosFromShip(systemConsole.transform.position, icon.transform, new Vector3(0f, 0f, UsableLayerOffset));
        }
    }

    // Creates icon for usables
    private static void CreateUsableIcon(IUsable usable)
    {
        // Special handling for emergency buttons
        if (usable.TryCast<SystemConsole>(out var systemConsole))
        {
            if (systemConsole.MinigamePrefab.name == "EmergencyMinigame")
            {
                var icon = CreateIcon(Utils.LoadSprite("BetterAmongUs.Resources.Images.Icons.Meeting.png", 500), "MeetingIcon");
                icon.color = new Color(1f, 1f, 1f, 0.7f);
                icon.transform.localScale = Vector3.one * 0.35f;
                SetPosFromShip(usable.Cast<MonoBehaviour>().transform.position, icon.transform, new Vector3(0f, 0f, UsableLayerOffset));
                return;
            }
            else
            {
                if (systemConsole.UseIcon is ImageNames.UseButton)
                {
                    return; // Skip default use button icons
                }
            }
        }

        // Create icon based on use button settings
        if (HudManager.Instance.UseButton.fastUseSettings.TryGetValue(usable.UseIcon, out var settings))
        {
            var icon = CreateIcon(settings.Image, "UsableIcon");
            icon.color = new Color(1f, 1f, 1f, 0.7f);
            icon.transform.localScale = Vector3.one * 0.35f;
            SetPosFromShip(usable.Cast<MonoBehaviour>().transform.position, icon.transform, new Vector3(0f, 0f, UsableLayerOffset));
        }
    }

    // Helper to create a new icon sprite renderer
    private static SpriteRenderer CreateIcon(Sprite sprite, string name = "Icon")
    {
        var go = new GameObject(name)
        {
            layer = LayerMask.NameToLayer("UI")
        };
        go.transform.SetParent(_icons);
        go.transform.localScale = Vector3.one;
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        return spriteRenderer;
    }

    // Helper to position icons on map relative to ship position
    private static void SetPosFromShip(Vector3 shipPos, Transform mapTransform, Vector3 offset = default)
    {
        Vector3 vector = shipPos;
        vector /= ShipStatus.Instance.MapScale;
        vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
        vector.z = -1f;
        mapTransform.transform.localPosition = new Vector3(vector.x + offset.x, vector.y + offset.y, offset.z);
    }
}