using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CatLaser;

internal sealed class PlayStep
{
    public PlayMode Mode { get; set; } = PlayMode.SmartPrey;
    public int Seconds { get; set; } = 20;
}
internal sealed class PlaySequence
{
    public string Name { get; set; } = "";
    public List<PlayStep> Steps { get; set; } = new();
}
internal sealed class PlaylistStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CatLaser", "playlists.json");
    public List<PlaySequence> Playlists { get; private set; } = new();
    public PlaylistStore()
    {
        try
        {
            if (File.Exists(_path))
                Playlists=JsonSerializer.Deserialize<List<PlaySequence>>(File.ReadAllText(_path)) ?? new();
            Playlists=Playlists.Where(p=>p.Name.Length is > 0 and <= 40 &&
                p.Steps is {Count: > 0 and <= 20} &&
                p.Steps.All(s=>Enum.IsDefined(s.Mode) && s.Seconds is >= 5 and <= 300))
                .Take(15).ToList();
        }
        catch (Exception) { Playlists=new(); }
        if (Playlists.Count==0)
            Playlists.Add(new PlaySequence {Name="CatLaser Mix", Steps=new()
            {
                new(){Mode=PlayMode.SmartPrey,Seconds=20},
                new(){Mode=PlayMode.Manual,Seconds=10}, // rest (laser hidden)
                new(){Mode=PlayMode.Ricochet,Seconds=30},
                new(){Mode=PlayMode.MouseHunt,Seconds=20}
            }});
    }
    public bool Save(PlaySequence value)
    {
        if (string.IsNullOrWhiteSpace(value.Name) || value.Name.Length>40 ||
            value.Steps.Count is < 1 or > 20 ||
            value.Steps.Any(s=>s.Seconds is < 5 or > 300)) return false;
        var prior=Playlists.FirstOrDefault(x=>x.Name.Equals(value.Name,StringComparison.OrdinalIgnoreCase));
        if (prior is null) { if(Playlists.Count>=15)return false; Playlists.Add(value); }
        else prior.Steps=value.Steps.Select(s=>new PlayStep{Mode=s.Mode,Seconds=s.Seconds}).ToList();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path+".tmp",JsonSerializer.Serialize(Playlists));
            File.Move(_path+".tmp",_path,true);
            return true;
        }
        catch (IOException){return false;}
        catch (UnauthorizedAccessException){return false;}
    }
}
