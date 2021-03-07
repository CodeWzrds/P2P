using System.Collections.Generic;
using System.Linq;

using Steamworks;

namespace ValheimP2P.Patches
{
    public class ZSteamMatchmaking
    {
        /// <summary>
        /// Adds the <see cref="ServerData.m_steamHostID"/> back in for server responses which results in the client using p2p for connecting to dedicated servers.
        /// </summary>
        /// <param name="___m_dedicatedServers">The dedicated servers list.</param>
        /// <param name="request"></param>
        /// <param name="iServer"></param>
        static void OnServerResponded_Postfix(ref List<ServerData> ___m_dedicatedServers, HServerListRequest request, int iServer)
        {
            var lastServer = ___m_dedicatedServers.Last();
            if (lastServer.m_steamHostID == 0)
            {
                var serverDetails = SteamMatchmakingServers.GetServerDetails(request, iServer);
                if (serverDetails.m_steamID.IsValid())
                {
                    lastServer.m_steamHostID = serverDetails.m_steamID.m_SteamID;
                }
                else
                {
                    ZLog.LogError($"[{PluginInfo.NAME}] Could not add steam host id to server {lastServer.m_name}. Failed to get valid steam id");
                }
            }
        }
    }
}
