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
            Log = Logger.GetLogger("ArmorRepair");
            Logger.SetLoggerLevel("ArmorRepair", LogLevel.Debug);

            try
            {
                CustomComponents.Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Settings = JsonConvert.DeserializeObject<ModSettings>(settingsJSON) ?? new ModSettings();
                harmony = new Harmony("io.github.citizenSnippy.ArmorRepair");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.LogDebug("Mod Initialized!");
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }

            Settings.Complete();
        }
    }
}
