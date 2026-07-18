using AmongUs.Data;
using Assets.InnerNet;
using BetterAmongUs.Enums;
using BetterAmongUs.Utilities;
using BetterAmongUs.Network.Configs;
using UnityEngine;

namespace BetterAmongUs.Modules;

/// <summary>
/// Represents a mod-specific news item with localization support.
/// </summary>
internal sealed class ModNews
{
    /// <summary>
    /// Gets the type of news item.
    /// </summary>
    internal NewsTypes NewsType { get; }

    /// <summary>
    /// Gets the unique identifier number for this news item.
    /// </summary>
    internal int Number { get; }

    /// <summary>
    /// Gets the title of the news item.
    /// </summary>
    internal string Title { get; }

    /// <summary>
    /// Gets the subtitle of the news item.
    /// </summary>
    internal string SubTitle { get; }

    /// <summary>
    /// Gets the short title for display in lists.
    /// </summary>
    internal string ShortTitle { get; }

    /// <summary>
    /// Gets or sets the dictionary of localized content by language ID.
    /// </summary>
    public Dictionary<int, string> Contents { get; set; } = [];

    /// <summary>
    /// Gets the publication date of the news item.
    /// </summary>
    internal string Date { get; }

    /// <summary>
    /// Gets the list of raw news data waiting to be processed.
    /// </summary>
    internal static List<NewsData> NewsDataToProcess { get; } = [];

    /// <summary>
    /// Gets the list of all processed mod news items.
    /// </summary>
    internal static List<ModNews> AllModNews { get; } = [];

    /// <summary>
    /// Initializes a new instance of the ModNews class.
    /// </summary>
    /// <param name="type">The type of news item.</param>
    /// <param name="number">The unique identifier number.</param>
    /// <param name="title">The title of the news.</param>
    /// <param name="subTitle">The subtitle of the news.</param>
    /// <param name="shortTitle">The short title for lists.</param>
    /// <param name="contents">Localized content by language ID.</param>
    /// <param name="date">The publication date.</param>
    internal ModNews(NewsTypes type, int number, string title, string subTitle, string shortTitle, Dictionary<int, string> contents, string date)
    {
        NewsType = type;
        Number = number;
        Title = title;
        SubTitle = subTitle;
        ShortTitle = shortTitle;
        Contents = contents;
        Date = date;

        AllModNews.Add(this);
    }

    /// <summary>
    /// Converts this ModNews instance to an AmongUs Announcement object.
    /// </summary>
    /// <returns>An Announcement object ready for display in-game.</returns>
    internal Announcement ToAnnouncement()
    {
        var announcement = new Announcement
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = "Error processing translation!".ToColor(Color.red),
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "ModNews"
        };

        if (Contents.TryGetValue((int)Translator.GetTargetLanguageId(), out var content))
        {
            announcement.Text = content;
        }
        else if (Contents.TryGetValue((int)SupportedLangs.English, out var englishContent))
        {
            announcement.Text = englishContent;
        }

        return announcement;
    }

    private static int _nextAnnouncementNumber;

    /// <summary>
    /// Processes all queued mod news data and creates ModNews instances.
    /// </summary>
    internal static void ProcessModNewsFiles()
    {
        AllModNews.Clear();

        _nextAnnouncementNumber = 110000;
        foreach (var config in NewsDataToProcess)
        {
            ParseModNewsContent(config, _nextAnnouncementNumber);
            _nextAnnouncementNumber++;
        }
    }

    /// <summary>
    /// Parses raw news data and creates a ModNews instance.
    /// </summary>
    /// <param name="config">The raw news data configuration.</param>
    /// <param name="nextAnnouncementNumber">The announcement number to assign.</param>
    private static void ParseModNewsContent(NewsData config, int nextAnnouncementNumber)
    {
        var type = (NewsTypes)config.Type;
        _ = new ModNews(type, nextAnnouncementNumber, config.Title, config.SubTitle, config.ListTitle, config.Contents, config.Date);
    }
}