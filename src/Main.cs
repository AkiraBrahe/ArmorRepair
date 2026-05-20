using BattleTech;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace ArmorRepair
{
    public class Main
    {
        internal static Harmony harmony;
        internal static string modDir;
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public static void Init(string directory, string settingsJSON)
        {
            modDir = directory;
            Settings = JsonConvert.DeserializeObject<ModSettings>(settingsJSON) ?? new ModSettings();
            Log = Logger.GetLogger("ArmorRepair");
            Logger.SetLoggerLevel("ArmorRepair", LogLevel.Debug);

            try
            {
                harmony = new Harmony("io.github.citizenSnippy.ArmorRepair");
                ApplyHarmonyPatches();
                SyncQuirkSettings();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        internal static void ApplyHarmonyPatches()
        {
            // --- BattleTech Extended ---
            /* Quirk Repair Cost Modifiers */
            harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SimGameState), "CreateMechRepairWorkOrder"), HarmonyPatchType.Prefix, "BEX.BattleTech.MechQuirks");
            harmony.Unpatch(AccessTools.Constructor(typeof(WorkOrderEntry_RepairMechStructure)), HarmonyPatchType.Prefix, "BEX.BattleTech.MechQuirks");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void SyncQuirkSettings()
        {
            var settings = Quirks.MechQuirks.modSettings;
            if (!settings.ClansDifficultToMaint && !settings.ClansNonStandard)
                Settings.ClanTechRepairCostMultiplier = 1.0f;
            if (!settings.ExtraTonnageRepairScaling)
                Settings.ScaleStructureRepairTimeByTonnage = false;
        }
    }
}