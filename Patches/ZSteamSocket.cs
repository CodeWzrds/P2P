using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

using HarmonyLib;
using Steamworks;

namespace ValheimP2P.Patches
{
    public class ZSteamSocket
    {
        /// <summary>
        /// The <see cref="MethodInfo"/> for <see cref="SteamGameServerNetworkingSockets.CreateListenSocketP2P(int, int, SteamNetworkingConfigValue_t[])"/>.
        /// </summary>
        static readonly MethodInfo _createListenSocketP2P
            = AccessTools.Method(typeof(SteamGameServerNetworkingSockets), "CreateListenSocketP2P", new[]
            {
                typeof(int),
                typeof(int),
                typeof(SteamNetworkingConfigValue_t[])
            });

        /// <summary>
        /// The <see cref="MethodInfo"/> for <see cref="SteamGameServerNetworkingSockets.CreateListenSocketIP(ref SteamNetworkingIPAddr, int, SteamNetworkingConfigValue_t[])"/>.
        /// </summary>
        static readonly MethodInfo _createListenSocketIP
            = AccessTools.Method(typeof(SteamGameServerNetworkingSockets), "CreateListenSocketIP", new[]
            {
                typeof(SteamNetworkingIPAddr).MakeByRefType(),
                typeof(int),
                typeof(SteamNetworkingConfigValue_t[])
            });

        /// <summary>
        /// Replaces the function call to <see cref="SteamGameServerNetworkingSockets.CreateListenSocketIP(ref SteamNetworkingIPAddr, int, SteamNetworkingConfigValue_t[])"/>
        /// with <see cref="SteamGameServerNetworkingSockets.CreateListenSocketP2P(int, int, SteamNetworkingConfigValue_t[])"/>.
        /// </summary>
        /// <param name="instructions">The <see cref="CodeInstruction"/>'s of <see cref="ZSteamSocket.StartHost"/></param>
        /// <returns></returns>
        static IEnumerable<CodeInstruction> StartHost_Transpile(IEnumerable<CodeInstruction> instructions)
        {
            if (_createListenSocketP2P is null || _createListenSocketIP is null)
            {
                ZLog.LogError($"[{PluginInfo.NAME}] Failed to patch ZSteamSocket.StartHost. Could not find CreateListenSocketP2P or CreateListenSocketIP.");
                return instructions;
            }

            var newInstructions = new List<CodeInstruction>(instructions);

            var createListenSocketInstrIdx = newInstructions.FindIndex(x => x.Calls(_createListenSocketIP));
            if (createListenSocketInstrIdx == -1)
                return newInstructions;

            newInstructions.RemoveRange(createListenSocketInstrIdx - 3, 4);
            newInstructions.InsertRange(createListenSocketInstrIdx - 3, new CodeInstruction[4]
            {
                new CodeInstruction(OpCodes.Ldc_I4, 0),
                new CodeInstruction(OpCodes.Ldc_I4, 0),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Call, _createListenSocketP2P)
            });

            return newInstructions;
        }
    }
}
