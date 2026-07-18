using AmongUs.Data.Player;
using Assets.InnerNet;
using BetterAmongUs.Enums;
using BetterAmongUs.Utilities;
using BetterAmongUs.Modules;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Globalization;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class AnnouncementPanelPatch
{
    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
    [HarmonyPrefix]
    private static void PlayerAnnouncementData_SetModAnnouncements_Prefix(PlayerAnnouncementData __instance, ref Il2CppReferenceArray<Announcement> aRange)
    {
        // Load and process mod news from github
        ModNews.ProcessModNewsFiles();

        // Sort mod news by date, newest first (higher number = newer date)
        ModNews.AllModNews.Sort((a1, a2) => DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)));

        // Convert all mod news to Announcement objects
        var finalAllNews = ModNews.AllModNews.Select(n => n.ToAnnouncement()).ToList();

        // Add original game announcements that aren't mod news
        foreach (var news in aRange)
        {
            if (!ModNews.AllModNews.Any(x => x.Number == news.Number))
            {
                finalAllNews.Add(news);
            }
        }

        // Sort combined list by date (newest first) using proper date parsing
        finalAllNews.Sort((a1, a2) =>
            DateTime.Compare(
                DateTime.Parse(a2.Date, null, DateTimeStyles.RoundtripKind),
                DateTime.Parse(a1.Date, null, DateTimeStyles.RoundtripKind)
            ));

        // Convert List<Announcement> back to Il2CppReferenceArray<Announcement> for game compatibility
        aRange = new Il2CppReferenceArray<Announcement>(finalAllNews.Count);
        for (int i = 0; i < finalAllNews.Count; i++)
        {
            aRange[i] = finalAllNews[i];
        }
    }

    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp))]
    [HarmonyPostfix]
    private static void AnnouncementPanel_SetUpPanel_Postfix(AnnouncementPanel __instance, Announcement announcement)
    {
        // Check if this is a mod announcement (mod news have numbers >= 100000)
        if (announcement.Number >= 100000)
        {
            // Create a GameObject to display the mod icon/label
            var obj = new GameObject("ModLabel");
            obj.transform.SetParent(__instance.transform);
            // Position in top-left corner of announcement panel
            obj.transform.localPosition = new Vector3(-0.8f, 0.13f, 0.5f);
            obj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

            // Add SpriteRenderer to display the icon
            var renderer = obj.AddComponent<SpriteRenderer>();
            var modNews = ModNews.AllModNews.Find(a => a.Number == announcement.Number);

            if (modNews != null)
            {
                // Load appropriate icon based on mod type
                switch (modNews.NewsType)
                {
                    case NewsTypes.BAU:
                        renderer.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Icon.png", 1225f);
                        break;
                }

                // Ensure icon stays within announcement panel bounds
                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
    }
}