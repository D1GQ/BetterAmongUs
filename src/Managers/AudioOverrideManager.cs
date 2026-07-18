using BetterAmongUs.Data;
using BetterAmongUs.Utilities;
using System.Reflection;
using UnityEngine;

namespace BetterAmongUs.Managers;

/// <summary>
/// Manages audio override files and their corresponding AudioClip data.
/// </summary>
internal static class AudioOverrideManager
{
    /// <summary>
    /// Collection of all discovered audio override data instances.
    /// </summary>
    internal static readonly List<AudioOverrideData> Overrides = [];

    internal static readonly AudioOverrideData Music_MainMenuSong = new("Music", "MainMenuSong.wav");

    internal static readonly AudioOverrideData Music_LobbySong = new("Music", "LobbySong.wav");

    internal static readonly AudioOverrideData Sounds_ChatNotification = new("Sounds/Chat", "ChatNotification.wav");

    internal static readonly AudioOverrideData Sounds_Kill = new("Sounds/Gameplay", "Kill.wav");

    internal static readonly MapAudioOverrideData Sounds_VentEnter = new("Sounds/Gameplay", "VentEnter.wav");

    internal static readonly MapAudioOverrideData Sounds_VentExit = new("Sounds/Gameplay", "VentExit.wav");

    internal static readonly MapAudioOverrideData Sounds_SabotageAlert = new("Sounds/Gameplay", "SabotageAlert.wav");

    internal static readonly AudioOverrideData Sounds_EmergencyMeetingStart = new("Sounds/Meeting", "EmergencyMeetingStart.wav");

    internal static readonly AudioOverrideData Sounds_EmergencyMeetingVote = new("Sounds/Meeting", "EmergencyMeetingVote.wav");

    internal static readonly AudioOverrideData Sounds_EmergencyMeetingVoteLockin = new("Sounds/Meeting", "EmergencyMeetingVoteLockin.wav");

    internal static readonly AudioOverrideData Sounds_EmergencyMeetingEnding = new("Sounds/Meeting", "EmergencyMeetingEnding.wav");

    /// <summary>
    /// Initializes the AudioOverrideManager by discovering all AudioOverrideData fields and loading their audio clips.
    /// </summary>
    /// <remarks>
    /// This method scans all static fields in AudioOverrideManager that inherit from AudioOverrideData,
    /// adds them to the Overrides collection, and loads their associated audio files from disk.
    /// </remarks>
    internal static void Initialize()
    {
        // Discover all AudioOverrideInfo fields
        var infoFields = typeof(AudioOverrideManager)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => typeof(AudioOverrideData).IsAssignableFrom(f.FieldType))
            .Select(f => f.GetValue(null) as AudioOverrideData)
            .Where(v => v != null);

        Overrides.Clear();
        foreach (var overrideData in infoFields)
        {
            if (overrideData != null)
            {
                Overrides.Add(overrideData);
            }
        }

        foreach (var overrideInfo in Overrides)
        {
            overrideInfo.Load();
        }
    }

    /// <summary>
    /// Represents an audio override file with its path and loaded AudioClip.
    /// Provides functionality for loading custom audio files from the file system.
    /// </summary>
    /// <param name="folderPath">The relative folder path to the audio file within the audio overrides directory.</param>
    /// <param name="fileName">The name of the audio file, including extension.</param>
    internal class AudioOverrideData(string folderPath, string fileName)
    {
        private readonly string _path = Path.Combine(Utils.GenerateDirectoryPath(folderPath), fileName);
        private AudioClip? audioClip;

        /// <summary>
        /// Loads the audio clip from disk. Creates the file and directory structure if they don't exist.
        /// </summary>
        internal virtual void Load()
        {
            if (string.IsNullOrEmpty(_path)) return;

            var fullPath = Path.Combine(BetterDataManager.Folders.audioOverridesFolderPath, _path);
            if (!Directory.Exists(fullPath))
            {
                string? directoryPath = Path.GetDirectoryName(fullPath);
                if (directoryPath == null) return;

                Directory.CreateDirectory(directoryPath);
            }
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath).Close();
            }
            audioClip = Utils.LoadWavFromDisk(fullPath);
            if (audioClip != null)
            {
                audioClip.name = "Override_" + Path.GetFileNameWithoutExtension(_path);
            }
        }

        /// <summary>
        /// Attempts to retrieve the loaded AudioClip.
        /// </summary>
        /// <param name="clip">When this method returns, contains the loaded AudioClip if successful; otherwise, null.</param>
        /// <returns>True if the AudioClip is available and loaded; otherwise, false.</returns>
        internal virtual bool TryGetAudioClip(out AudioClip? clip)
        {
            if (audioClip != null)
            {
                clip = audioClip;
                return true;
            }

            clip = null;
            return false;
        }

        /// <summary>
        /// Determines whether an audio clip is currently associated with this instance.
        /// </summary>
        /// <returns><see langword="true"/> if an audio clip is loaded and present; otherwise, <see langword="false"/>.</returns>
        internal virtual bool HasAudioClip()
        {
            return audioClip != null;
        }
    }

    /// <summary>
    /// Represents a map specific audio override that loads different audio files for different maps.
    /// </summary>
    /// <param name="folderPath">The relative folder path to the audio files within the audio overrides directory.</param>
    /// <param name="fileName">The base name of the audio file, which will be prefixed with map names.</param>
    internal sealed class MapAudioOverrideData(string folderPath, string fileName) : AudioOverrideData(folderPath, fileName)
    {
        /// <summary>
        /// Enumeration of map types for audio override categorization.
        /// </summary>
        private enum MapTyped
        {
            Ships,
            MiraHQ,
            Polus,
            Fungle
        }

        private readonly string _folderPath = folderPath;
        private readonly string _fileName = fileName;
        private readonly Dictionary<MapTyped, AudioOverrideData> audioOverrideDatas = [];

        /// <summary>
        /// Loads map specific audio overrides for all supported map types.
        /// </summary>
        internal override void Load()
        {
            foreach (var map in Enum.GetValues(typeof(ShipStatus.MapType)))
            {
                if (map is ShipStatus.MapType mapType)
                {
                    var audioData = new AudioOverrideData(Path.Combine(Utils.GenerateDirectoryPath(_folderPath), Path.GetFileNameWithoutExtension(_fileName)), $"{(MapTyped)mapType}_" + _fileName);
                    audioOverrideDatas[(MapTyped)mapType] = audioData;
                    audioData.Load();
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve the map appropriate AudioClip based on the currently active map.
        /// </summary>
        /// <param name="clip">When this method returns, contains the map specific AudioClip if successful; otherwise, null.</param>
        /// <returns>True if a map-specific AudioClip is available for the current map; otherwise, false.</returns>
        internal override bool TryGetAudioClip(out AudioClip? clip)
        {
            if (ShipStatus.Instance != null)
            {
                if (audioOverrideDatas.TryGetValue((MapTyped)ShipStatus.Instance.Type, out var audioData))
                {
                    if (audioData.TryGetAudioClip(out var audio))
                    {
                        clip = audio;
                        return true;
                    }
                }
            }

            clip = null;
            return false;
        }
    }
}