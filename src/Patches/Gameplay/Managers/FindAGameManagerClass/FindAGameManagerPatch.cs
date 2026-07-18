using BetterAmongUs.Utilities;
using HarmonyLib;
using InnerNet;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Managers;

[HarmonyPatch]
internal static class FindAGameManagerPatch
{
    public static Scroller? Scroller;

    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.Start))]
    [HarmonyPrefix]
    private static void FindAGameManager_Start_Prefix(FindAGameManager __instance)
    {
        // Apply custom UI colors to all Find Game menu buttons
        __instance.refreshButton.gameObject.SetUIColors();
        __instance.BackButton.gameObject.SetUIColors();
        __instance.clearFilterButton.gameObject.SetUIColors("Disabled");
        __instance.serverButton.gameObject.SetUIColors("Inactive", "Disabled", "Background");
        __instance.serverButton.activeTextColor = Color.cyan * 0.3f;

        // Modify game container hover colors
        foreach (var con in __instance.gameContainers)
        {
            var roll = con.GetComponent<ButtonRolloverHandler>();
            roll.OverColor = (roll.OverColor * 0.6f) + (Color.green * 0.5f);
        }

        // Create scrollable container for game listings
        var prefab = __instance.gameContainers[4];
        var list = new GameObject("GameListScroller");
        list.transform.SetParent(prefab.transform.parent);

        // Set up scroller component for vertical scrolling
        Scroller = list.AddComponent<Scroller>();
        Scroller.Inner = list.transform;
        Scroller.MouseMustBeOverToScroll = true;
        var box = prefab.transform.parent.gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(100f, 100f);
        Scroller.ClickMask = box;
        Scroller.ScrollWheelSpeed = 0.3f;
        Scroller.SetYBoundsMin(0f);
        Scroller.SetYBoundsMax(3.5f);
        Scroller.allowY = true;

        // Move existing game containers into scrollable list
        foreach (var con in __instance.gameContainers)
        {
            con.transform.SetParent(list.transform);
            var oldPos = con.transform.position;
            con.transform.position = new Vector3(oldPos.x, oldPos.y, 25);
        }

        // Add more game containers to support longer server lists
        var oldGameContainers = __instance.gameContainers.ToList();

        for (int i = 0; i < 5; i++)
        {
            var GameContainer = UnityEngine.Object.Instantiate(prefab, list.transform);
            var oldPos = GameContainer.transform.position;
            GameContainer.transform.position = new Vector3(oldPos.x, oldPos.y - 0.75f * (i + 1), 25);
            oldGameContainers.Add(GameContainer);
        }

        __instance.gameContainers = oldGameContainers.ToArray();

        // Create visual cutoff for scrolling content
        var cutOffTop = CreateBlackSquareSprite();
        cutOffTop.transform.SetParent(list.transform.parent);
        cutOffTop.transform.localPosition = new Vector3(0, 3, 1);
        cutOffTop.transform.localScale = new Vector3(1500, 200, 100);
    }

    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.RefreshList))]
    [HarmonyPostfix]
    private static void FindAGameManager_RefreshList_Postfix(FindAGameManager __instance)
    {
        // Reset scroll position when refreshing game list
        Scroller?.ScrollRelative(new(0f, -100f));
    }

    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.HandleList))]
    [HarmonyPostfix]
    private static void FindAGameManager_HandleList_Postfix(FindAGameManager __instance, HttpMatchmakerManager.FindGamesListFilteredResponse response)
    {
        // Clear existing containers before populating with new data
        __instance.ResetContainers();
        GameListing[] games = response.Games.ToArray();

        // Sort games: first by player count (highest first), then by host name
        games = [.. games.OrderByDescending(game => game.PlayerCount).ThenBy(game => game.TrueHostName)];
        int gameNum = 0;
        int count = 0;

        // Populate visible containers with available games
        while (count < __instance.gameContainers.Length && count < games.Count())
        {
            if (games[count].Options != null)
            {
                __instance.gameContainers[gameNum].gameObject.SetActive(true);
                __instance.gameContainers[gameNum].SetGameListing(games[count]);
                __instance.gameContainers[gameNum].SetupGameInfo();
                gameNum++;
            }
            count++;
        }

        // Add true host name and platform info to each game container
        foreach (var container in __instance.gameContainers)
        {
            Transform child = container.transform.Find("Container");
            Transform tmproObject = child.Find("TrueHostName_TMP");

            // Use existing TMP component or create new one
            TMP_Text tmpro = tmproObject != null
                ? tmproObject.GetComponent<TextMeshPro>()
                : CreateNewTextMeshPro(child);

            tmpro.font = container.capacity.font;
            tmpro.fontSize = 3f;
            tmpro.text = FormatGameInfoText(container.gameListing);
        }
    }

    private static TMP_Text CreateNewTextMeshPro(Transform parent)
    {
        // Create new TextMeshPro component for displaying host info
        var tmproObject = new GameObject("TrueHostName_TMP").transform;
        tmproObject.SetParent(parent, true);

        // Position text in container
        var aspectPos = tmproObject.gameObject.AddComponent<AspectPosition>();
        aspectPos.Alignment = AspectPosition.EdgeAlignments.Center;
        aspectPos.anchorPoint = new Vector2(0.2f, 0.5f);
        aspectPos.DistanceFromEdge = new Vector3(10.9f, -2.17f, -2f);
        aspectPos.AdjustPosition();

        return tmproObject.gameObject.AddComponent<TextMeshPro>();
    }

    private static string FormatGameInfoText(GameListing listing)
    {
        // Format game info text with host name, platform, and game code
        var hostStr = !string.IsNullOrEmpty(listing.TrueHostName) ? listing.TrueHostName : listing.HostName;
        return @$"{hostStr}{Environment.NewLine}<size=65%>{Utils.GetPlatformName(listing.Platform)} ({GameCode.IntToGameName(listing.GameId)})";
    }

    private static SpriteRenderer CreateBlackSquareSprite()
    {
        // Create a black sprite to visually cut off scrolling content
        var square = new GameObject("CutOffTop");
        var renderer = square.AddComponent<SpriteRenderer>();
        Texture2D texture = new(100, 100);
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        renderer.sprite = sprite;
        square.transform.localScale = new Vector3(100, 100, 1);
        return renderer;
    }
}