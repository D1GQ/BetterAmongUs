using BetterAmongUs.Modules.Support;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.SceneManagement;
using BetterAmongUs.Utilities.Extension;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class ServerDropdownPatch
{
    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.Start))]
    [HarmonyPrefix]
    private static void FindAGameManager_Start_Prefix(FindAGameManager __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_ServerDropDown))
            return;

        var aspectPosition = __instance.serverDropdown.transform.parent.GetComponent<AspectPosition>();
        if (aspectPosition != null)
        {
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.Top;
            aspectPosition.anchorPoint = Vector3.zero;
            aspectPosition.DistanceFromEdge = new Vector3(-1.2f, 0.3f, 0f);
            aspectPosition.AdjustPosition();
        }

        __instance.modeText.transform.localPosition -= new Vector3(0.4f, 0f, 0f);
    }

    [HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    [HarmonyPrefix]
    private static bool ServerDropdown_FillServerOptions_Prefix(ServerDropdown __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_ServerDropDown)) return true;

        __instance.background.size = new Vector2(5, 1);

        int num = 0;
        int column = 0;
        int maxPerColumn = SceneManager.GetActiveScene().name == "FindAGame" ? 10 : 6;
        const float columnWidth = 3.5f;
        const float buttonSpacing = 0.5f;

        // Get all available regions except current one
        var regions = ServerManager.Instance.AvailableRegions.OrderBy(ServerManager.DefaultRegions.Contains).ToList();

        // Calculate total columns needed
        int totalColumns = Mathf.Max(1, Mathf.CeilToInt(regions.Count / (float)maxPerColumn));
        int rowsInLastColumn = regions.Count % maxPerColumn;
        int maxRows = (regions.Count > maxPerColumn) ? maxPerColumn : regions.Count;

        foreach (IRegionInfo regionInfo in regions)
        {
            if (ServerManager.Instance.CurrentRegion.Name == regionInfo.Name)
            {
                __instance.defaultButtonSelected = __instance.firstOption;
                __instance.firstOption.ChangeButtonText(TranslationController.Instance.GetStringWithDefault(regionInfo.TranslateName, regionInfo.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
                continue;
            }

            IRegionInfo region = regionInfo;
            ServerListButton serverListButton = __instance.ButtonPool.Get<ServerListButton>();

            // Calculate position based on column and row
            float xPos = (column - (totalColumns - 1) / 2f) * columnWidth;
            float yPos = __instance.y_posButton - buttonSpacing * (num % maxPerColumn);

            serverListButton.transform.localPosition = new Vector3(xPos, yPos, -1f);
            serverListButton.transform.localScale = Vector3.one;
            serverListButton.Text.enableAutoSizing = true;
            serverListButton.Text.text = TranslationController.Instance.GetStringWithDefault(
                regionInfo.TranslateName,
                regionInfo.Name,
                new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            serverListButton.Text.ForceMeshUpdate(false, false);
            serverListButton.Button.OnClick.RemoveAllListeners();
            serverListButton.Button.OnClick.AddListener(() => __instance.ChooseOption(region));
            __instance.controllerSelectable.Add(serverListButton.Button);

            // Move to next column if current column is full
            num++;
            if (num % maxPerColumn == 0)
            {
                column++;
            }
        }

        // Calculate background dimensions
        float backgroundHeight = 1.2f + buttonSpacing * (maxRows - 1);
        float backgroundWidth = (totalColumns > 1) ?
            (columnWidth * (totalColumns - 1) + __instance.background.size.x) :
            __instance.background.size.x;

        __instance.background.transform.localPosition = new Vector3(
            0f,
            __instance.initialYPos - (backgroundHeight - 1.2f) / 2f,
            0f);
        __instance.background.size = new Vector2(backgroundWidth, backgroundHeight);

        return false;
    }
}
