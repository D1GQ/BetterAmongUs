using BetterAmongUs.Utilities;
using InnerNet;

namespace BetterAmongUs.Structs;

/// <summary>
/// Represents temporary client data for caching or snapshot purposes.
/// </summary>
/// <param name="clientData">The client data to copy values from.</param>
internal readonly struct TempClientData(ClientData clientData) : IDisposable
{
    private static readonly Dictionary<string, TempClientData> friendCodeLookup = [];
    private static readonly Dictionary<string, TempClientData> puidLookup = [];

    /// <summary>
    /// Gets or creates a cached instance of TempClientData for the specified client data.
    /// </summary>
    /// <param name="clientData">The client data to get or create a cached instance for.</param>
    /// <returns>A cached TempClientData instance for the specified client.</returns>
    internal static TempClientData Get(ClientData clientData)
    {
        TempClientData tempClientData;
        if (!friendCodeLookup.TryGetValue(clientData.FriendCode, out tempClientData))
        {
            if (!puidLookup.TryGetValue(clientData.ProductUserId, out tempClientData))
            {
                tempClientData = new TempClientData(clientData);
                if (!string.IsNullOrEmpty(clientData.FriendCode))
                {
                    friendCodeLookup[clientData.FriendCode] = tempClientData;
                }
                if (!string.IsNullOrEmpty(clientData.ProductUserId))
                {
                    puidLookup[clientData.ProductUserId] = tempClientData;
                }
            }
        }

        return tempClientData;
    }

    /// <summary>
    /// Disposes the TempClientData instance by removing it from all lookup caches.
    /// </summary>
    public void Dispose()
    {
        friendCodeLookup.Remove(FriendCode);
        puidLookup.Remove(Puid);
    }

    /// <summary>
    /// Gets the unique identifier of the client.
    /// </summary>
    internal readonly int Id = clientData.Id;

    /// <summary>
    /// Gets the product user ID (PUID) of the client.
    /// </summary>
    internal readonly string Puid = clientData.ProductUserId;

    /// <summary>
    /// Gets the hashed product user ID.
    /// </summary>
    internal readonly string HashPuid = Utils.GetHashStr(clientData.ProductUserId);

    /// <summary>
    /// Gets the friend code of the client.
    /// </summary>
    internal readonly string FriendCode = clientData.FriendCode;

    /// <summary>
    /// Gets the player name of the client.
    /// </summary>
    internal readonly string PlayerName = clientData.PlayerName;
}