using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CatLaser;

internal sealed class LaserPreset
{
    public string Name { get; set; } = "";
    public LaserOptions Options { get; set; } = new();
}

internal sealed class PresetStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CatLaser", "presets.json");

    public List<LaserPreset> Presets { get; private set; } = new();

    public PresetStore()
    {
        try
        {
            if (File.Exists(_path))
                Presets = JsonSerializer.Deserialize<List<LaserPreset>>(File.ReadAllText(_path)) ?? new();
            Presets = Presets.Where(p => p.Options != null && !string.IsNullOrWhiteSpace(p.Name))
                .Take(25).ToList();
            foreach (var preset in Presets) preset.Options.Sanitize();
        }
        catch (Exception) { Presets = new(); } // Bad preferences must never prevent launch.
    }

    public bool Save(string name, LaserOptions options)
    {
        name = name.Trim();
        if (name.Length == 0 || name.Length > 40) return false;
        var current = Presets.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (current == null)
        {
            if (Presets.Count >= 25) return false;
            Presets.Add(new LaserPreset { Name = name, Options = options.Clone() });
        }
        else current.Options = options.Clone();
        Write();
        return true;
    }

    public void Delete(string name)
    {
        Presets.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        Write();
    }

    private void Write()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(Presets, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, _path, true);
        }
        catch (IOException) { } // User may have a locked/read-only folder.
        catch (UnauthorizedAccessException) { }
    }
}
