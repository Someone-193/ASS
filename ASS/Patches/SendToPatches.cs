namespace ASS.Patches
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using ASS.Settings;
    using HarmonyLib;
    using LabApi.Features.Wrappers;
    using UserSettings.ServerSpecific;
    using static HarmonyLib.AccessTools;

    [HarmonyPatch(typeof(ServerSpecificSettingsSync))]
    public class SendToPatches
    {
        [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(Player))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler1()
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), [typeof(ReferenceHub)]));
            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(ASSNetworking), nameof(ASSNetworking.SendToPlayer), [typeof(Player), typeof(bool), typeof(bool)]));
        }

        [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(Player), typeof(ServerSpecificSettingBase[]), typeof(int?))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2()
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), [typeof(ReferenceHub)]));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(ASSNetworking), nameof(ASSNetworking.SendSSSIncludingASS)));
        }

        [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToAll))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3()
        {
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(ASSNetworking), nameof(ASSNetworking.SendToAll)));
        }

        [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler4()
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, Method(typeof(ASSNetworking), nameof(ASSNetworking.SendByFilter)));
        }
    }
}