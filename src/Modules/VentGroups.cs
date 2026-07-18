using BetterAmongUs.Data.Config;
using UnityEngine;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides functionality for grouping vents and assigning colors to vent groups.
/// </summary>
internal static class VentGroups
{
    /// <summary>
    /// Dictionary mapping each vent ID to the lowest ID in its group.
    /// </summary>
    private static readonly Dictionary<int, int> ventToLowestId = [];

    /// <summary>
    /// Dictionary mapping group IDs (lowest vent ID in the group) to their assigned colors.
    /// </summary>
    private static readonly Dictionary<int, Color> groupColors = [];

    /// <summary>
    /// Calculates all vent groups by performing a breadth-first search through connected vents.
    /// </summary>
    /// <param name="vents">Array of all vents in the scene to process.</param>
    /// <remarks>
    /// Groups vents that are connected via Left, Right, or Center references.
    /// Each group is assigned a color, and the lowest vent ID in the group becomes the group identifier.
    /// </remarks>
    internal static void CalculateAllVentGroups(Vent[] vents)
    {
        ventToLowestId.Clear();
        groupColors.Clear();

        HashSet<Vent> unprocessedVents = [.. vents];

        HashSet<int> groupLowestIds = [];
        Dictionary<int, List<Vent>> groups = [];

        while (unprocessedVents.Count > 0)
        {
            Vent startVent = unprocessedVents.First();

            HashSet<Vent> group = [];
            Queue<Vent> toVisit = new();
            int lowestId = startVent.Id;

            toVisit.Enqueue(startVent);

            while (toVisit.Count > 0)
            {
                Vent current = toVisit.Dequeue();
                if (group.Contains(current)) continue;

                group.Add(current);
                unprocessedVents.Remove(current);

                if (current.Id < lowestId)
                {
                    lowestId = current.Id;
                }

                Vent[] connections = { current.Left, current.Right, current.Center };
                foreach (var connection in connections)
                {
                    if (connection != null && !group.Contains(connection) && !toVisit.Contains(connection))
                    {
                        toVisit.Enqueue(connection);
                    }
                }
            }

            groupLowestIds.Add(lowestId);
            groups[lowestId] = [.. group];
        }

        Color[] colorGroups =
        [
            Color.red,
            Color.green,
            Color.blue,
            Color.gray,
            Color.magenta,
            Color.cyan,
            Color.white
        ];

        for (int i = 0; i < groupLowestIds.Count; i++)
        {
            int lowestId = groupLowestIds.ElementAt(i);
            int colorIndex = i % colorGroups.Length;
            groupColors[lowestId] = colorGroups[colorIndex] + (new Color(1f, 1f, 1f, 0f) * 0.2f);
        }

        foreach (var kvp in groups)
        {
            int lowestId = kvp.Key;
            foreach (var vent in kvp.Value)
            {
                ventToLowestId[vent.Id] = lowestId;
            }
        }
    }

    /// <summary>
    /// Gets the group color for a specific vent.
    /// </summary>
    /// <param name="vent">The vent to get the group color for.</param>
    /// <returns>
    /// The color assigned to the vent's group, or <see cref="Color.black"/> if the vent hasn't been processed.
    /// </returns>
    internal static Color GetVentGroupColor(Vent vent)
    {
        if (!BAUConfigs.VentColorGroups.Value)
        {
            return Color.gray;
        }

        if (ventToLowestId.TryGetValue(vent.Id, out int lowestId))
        {
            return groupColors[lowestId];
        }

        return Color.black;
    }
}