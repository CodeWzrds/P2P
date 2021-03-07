using System.Collections.Generic;
using System.Diagnostics;
using System;

using Steamworks;
using HarmonyLib;
using BepInEx;

namespace ValheimP2P
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    [BepInProcess("valheim.exe")]
    [BepInProcess("valheim_server.exe")]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// The <see cref="Harmony"/> instanced used for patching.
        /// </summary>
        readonly Harmony _harmony = new Harmony(PluginInfo.GUID);

        /// <summary>
        /// Gets called when the script is being loaded.
        /// </summary>
        void Awake()
        {
            var procName = Process.GetCurrentProcess().ProcessName;
            if (procName.Equals("valheim_server", StringComparison.OrdinalIgnoreCase))
            {
                var original  = AccessTools.Method(typeof(ZSteamSocket), "StartHost");
                if (original is null)
                {
                    ZLog.LogError($"[{PluginInfo.NAME}] Coult not find ZSteamSocket.StartHost");
                    return;
                }

                _harmony.Patch(
                      original: original,
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches.ZSteamSocket), "StartHost_Transpile", new[]
                    {
                        typeof(IEnumerable<CodeInstruction>)
                    }))
                );

                ZLog.Log($"[{PluginInfo.NAME}] Patched Server!");
            }
            else
            {
                var original = AccessTools.Method(typeof(ZSteamMatchmaking), "OnServerResponded", new[]
                {
                    typeof(HServerListRequest),
                    typeof(int)
                });
                if (original is null)
                {
                    ZLog.LogError($"[{PluginInfo.NAME}] Coult not find ZSteamMatchmaking.OnServerResponded");
                    return;
                }

                _harmony.Patch(
                    original: original,
                     postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches.ZSteamMatchmaking), "OnServerResponded_Postfix", new[]
                     {
                         typeof(List<ServerData>).MakeByRefType(),
                         typeof(HServerListRequest),
                         typeof(int)
                     }))
                );

                ZLog.Log($"[{PluginInfo.NAME}] Patched Client!");
            }
        }

        /// <summary>
        /// Gets called when the script gets disabled or unloads.
        /// </summary>
        void OnDisable() => _harmony.UnpatchAll(PluginInfo.GUID);
    }

    public static class PluginInfo
    {
        public const string NAME    = "ValheimP2P";
        public const string VERSION = "1.0.2";
        public const string GUID    = "org.bepinex.valheim_peer2peer";
    }
}
