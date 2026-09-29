using System;
using Microsoft.Win32;

namespace IdeaUnstuck.AddIn
{
    /// <summary>Per-Windows-user choices: text size and whether the welcome was dismissed.</summary>
    public interface IPaneSettings
    {
        int TextSize { get; set; }
        bool WelcomeDismissed { get; set; }
    }

    /// <summary>Stored in HKCU\Software\IDEA\Unstuck, which every Windows user has and can write.</summary>
    public sealed class RegistryPaneSettings : IPaneSettings
    {
        const string Key = @"Software\IDEA\Unstuck";
        static int Read(string name, int fallback)
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(Key)) return k?.GetValue(name) is int v ? v : fallback; }
            catch { return fallback; }
        }
        static void Write(string name, int value)
        {
            try { using (var k = Registry.CurrentUser.CreateSubKey(Key)) k.SetValue(name, value, RegistryValueKind.DWord); }
            catch { /* A locked-down profile keeps the default; never interrupt a student over this. */ }
        }
        public int TextSize { get => Math.Max(0, Math.Min(2, Read("TextSize", 0))); set => Write("TextSize", value); }
        public bool WelcomeDismissed { get => Read("WelcomeDismissed", 0) == 1; set => Write("WelcomeDismissed", value ? 1 : 0); }
    }

    public sealed class MemoryPaneSettings : IPaneSettings
    {
        public int TextSize { get; set; }
        public bool WelcomeDismissed { get; set; }
    }
}
