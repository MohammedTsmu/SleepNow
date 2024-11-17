using System;
using System.IO;
using Newtonsoft.Json;

namespace SleepNow
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SleepNow", "settings.json");

        public static AppSettings CurrentSettings { get; private set; } = new AppSettings();

        static SettingsManager()
        {
            LoadSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save settings.", ex);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    CurrentSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                CurrentSettings = new AppSettings(); // Load defaults if there's an error
            }
        }
    }

    public class AppSettings
    {
        public DateTime SleepTime { get; set; } = DateTime.Now.AddHours(1); // Default: 1 hour from now
        public int PreAlertMinutes { get; set; } = 15; // Default: 15 minutes
        public bool RestrictChanges { get; set; } = false; // Restrict time changes
        public int RestrictionDuration { get; set; } = 120; // Default restriction: 2 hours
    }
}
