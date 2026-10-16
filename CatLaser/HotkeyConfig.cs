using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Forms = System.Windows.Forms;

namespace CatLaser;

internal sealed class HotkeyBinding
{
    public int Id { get; set; }
    public uint Modifiers { get; set; } = 3; // Ctrl+Alt
    public int VirtualKey { get; set; }
}

internal static class HotkeyConfig
{
    public static readonly string[] Actions = { "toggle", "effect", "color", "faster", "slower", "mode" };
    public static readonly (string Label, uint Value)[] Modifiers =
    {
        ("Ctrl + Alt", 3), ("Ctrl + Shift", 6), ("Alt + Shift", 5)
    };
    public static readonly (string Label, int Value)[] Keys =
    {
        ("L", (int)Forms.Keys.L), ("E", (int)Forms.Keys.E), ("C", (int)Forms.Keys.C),
        ("M", (int)Forms.Keys.M), ("F", (int)Forms.Keys.F), ("G", (int)Forms.Keys.G),
        ("H", (int)Forms.Keys.H), ("J", (int)Forms.Keys.J),
        ("Up", (int)Forms.Keys.Up), ("Down", (int)Forms.Keys.Down),
        ("Left", (int)Forms.Keys.Left), ("Right", (int)Forms.Keys.Right),
        ("F6", (int)Forms.Keys.F6), ("F7", (int)Forms.Keys.F7),
        ("F8", (int)Forms.Keys.F8), ("F9", (int)Forms.Keys.F9)
    };
    private static readonly string PathName = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CatLaser", "hotkeys.json");

    public static List<HotkeyBinding> Defaults() => new()
    {
        new() { Id=1, VirtualKey=(int)Forms.Keys.L },
        new() { Id=2, VirtualKey=(int)Forms.Keys.E },
        new() { Id=3, VirtualKey=(int)Forms.Keys.C },
        new() { Id=4, VirtualKey=(int)Forms.Keys.Up },
        new() { Id=5, VirtualKey=(int)Forms.Keys.Down },
        new() { Id=6, VirtualKey=(int)Forms.Keys.M }
    };

    public static bool Valid(IReadOnlyList<HotkeyBinding> bindings)
    {
        if (bindings.Count != 6) return false;
        if (bindings.Select(b => b.Id).Distinct().Count() != 6 ||
            bindings.Any(b => b.Id is < 1 or > 6)) return false;
        if (bindings.Any(b => !Modifiers.Any(m => m.Value == b.Modifiers) ||
                              !Keys.Any(k => k.Value == b.VirtualKey))) return false;
        return bindings.Select(b => (b.Modifiers,b.VirtualKey)).Distinct().Count() == 6;
    }

    public static List<HotkeyBinding> Load()
    {
        try
        {
            if (File.Exists(PathName))
            {
                var list = JsonSerializer.Deserialize<List<HotkeyBinding>>(File.ReadAllText(PathName));
                if (list is not null && Valid(list)) return list.OrderBy(b => b.Id).ToList();
            }
        }
        catch (Exception) { } // Never block startup due to corrupt local preferences.
        return Defaults();
    }

    public static bool Save(IReadOnlyList<HotkeyBinding> bindings)
    {
        if (!Valid(bindings)) return false;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
            string temp = PathName + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(bindings));
            File.Move(temp, PathName, true);
            return true;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    public static string Display(HotkeyBinding b)
    {
        string mods = Modifiers.FirstOrDefault(x => x.Value == b.Modifiers).Label ?? "Ctrl + Alt";
        string key = Keys.FirstOrDefault(x => x.Value == b.VirtualKey).Label ?? "L";
        return mods + " + " + key;
    }
}
