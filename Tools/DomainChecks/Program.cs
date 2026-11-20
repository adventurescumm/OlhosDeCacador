using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using BemTeVi;

static class Checks
{
    static int checks;
    static JsonSerializerOptions json = new JsonSerializerOptions { IncludeFields = true, IgnoreReadOnlyProperties = true };
    static string Encode<T>(T value) => JsonSerializer.Serialize(value, json);
    static T Decode<T>(string value)
    { try { return JsonSerializer.Deserialize<T>(value, json); } catch (JsonException e) { throw new ArgumentException("Invalid JSON", e); } }
    static void Check(bool value, string message)
    { checks++; if (!value) throw new Exception(message); }
    static void Throws(Action action, string message)
    { try { action(); } catch (ArgumentException) { checks++; return; } throw new Exception(message); }
    static Catalog Load(string path) => Decode<Catalog>(File.ReadAllText(path));
    public static void Main(string[] args)
    {
        string path = args[0]; var catalog = Load(path); var progress = new Progress(catalog);
        Check(catalog.maps.SelectMany(m => m.AllTargets).Count() == 162, "162 targets");
        foreach (var map in catalog.maps)
        {
            for (int round = 0; round < 3; round++)
            {
                Check(map.currentRound == round, "Round order");
                Check(!progress.Complete(map), "Incomplete round cannot finish");
                Check(!progress.Collect("unknown"), "Unknown click");
                foreach (var target in map.AllTargets.Where(p => p.round > round))
                    Check(!progress.Collect(target.id), "Future round must not count");
                foreach (var target in map.Targets.ToArray())
                {
                    Check(progress.Collect(target.id), "New discovery");
                    Check(!progress.Collect(target.id), "Repeat click");
                }
                if (round == 0) foreach (var gate in map.gates)
                { Check(progress.Use(map, gate.id, gate.tokenId), "Gate opens"); Check(!progress.Use(map, gate.id, gate.tokenId), "Gate idempotent"); }
                Check(progress.Complete(map), "Round completion");
                progress = new Progress(catalog, Decode<SaveData>(Encode(progress.Data)));
                Check(progress.CompletedRounds(map) == round + 1, "Round survives save");
            }
        }
        Check(progress.Finished, "Nine rounds finish");
        Check(progress.Data.found.Count == 162, "All discoveries retained");
        progress.ResetMap(catalog.maps[0]);
        Check(progress.Data.found.Count == 108 && progress.Data.solved.Count == 6 && progress.Data.opened.Count == 4, "Reset one world isolated");
        Check(catalog.maps[0].currentRound == 0 && catalog.maps[1].currentRound == 2, "Reset rounds isolated");
        foreach(var map in catalog.maps)
        foreach(var stage in map.rooms.Concat(new[] { map.junction, map.challenge }))
        {
            foreach(var exit in stage.objects.Where(p => p.action == "exit"))
                Check(progress.CanTraverse(map, stage, exit.destination), "Physical passages stay available");
            Check(!progress.CanTraverse(map, stage, "missing"), "No invalid teleport");
        }
        var original = Load(path); var old = new SaveData();
        old.found.AddRange(original.maps[0].Targets.Select(t => t.id)); old.solved.Add(original.maps[0].id);
        var migrated = new Progress(original, old);
        Check(original.maps[0].currentRound == 1 && migrated.CompletedRounds(original.maps[0]) == 1, "v0.10 round-one completion migrates");
        Throws(() => new Progress(original, new SaveData { schema = 99 }), "Future schema rejected");
        var archive = new PlayerArchive(); var ana = archive.Create("Ana");
        var p1 = new Progress(original); p1.Collect(original.maps[0].Targets.First().id); ana.progress = p1.Data;
        ana.stageId = original.maps[0].rooms[0].id;
        var bruno = archive.Create("Bruno");
        Check(ana.id != bruno.id && bruno.progress.found.Count == 0, "Profiles isolated");
        Throws(() => archive.Create(" ana "), "Duplicate name rejected");
        Throws(() => archive.Create("  "), "Empty name rejected");
        archive.activeId = ana.id;
        int activeRound = original.maps[0].currentRound;
        archive.Validate(original);
        Check(original.maps[0].currentRound == activeRound, "Validation must not switch another profile's rounds");
        ana.puzzles.Add(new PuzzleSave { mapId = original.maps[0].id, round = 0, digits = new[] { 2, 4, 3 } });
        string folder = Path.Combine(Path.GetTempPath(), "bemtevi-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string savePath = Path.Combine(folder, "players.json");
            var store = new PlayerSaveStore(savePath, Encode, Decode<PlayerArchive>, Encode, Decode<SaveEnvelope>, a => a.Validate(original));
            Check(store.Load().players.Count == 0 && !store.Unreadable, "First launch");
            store.Save(archive); var loaded = store.Load();
            Check(loaded.players.Count == 2 && loaded.Active.name == "Ana", "Profiles and selection persist");
            Check(loaded.Active.stageId == ana.stageId && loaded.Active.progress.found.Count == 1, "Location and finds persist");
            Check(loaded.Active.puzzles[0].digits.SequenceEqual(new[] { 2, 4, 3 }), "Puzzle state persists");
            Check(File.Exists(savePath + ".bak") && !File.Exists(savePath + ".tmp"), "Backup written and temp cleared");
            archive.Create("Carla"); store.Save(archive);
            Check(store.Load().players.Count == 3, "New snapshot committed");
            File.WriteAllText(savePath, "{broken"); loaded = store.Load();
            Check(store.Recovered && loaded.players.Count == 2, "Corruption recovers prior valid snapshot");
            store.Save(loaded);
            Check(store.Load().players.Count == 2 && !store.Recovered, "Recovered copy repaired");
            Check(Directory.GetFiles(folder, "*.corrupt-*").Length == 1, "Corrupt original retained");
            File.WriteAllText(savePath, File.ReadAllText(savePath).Replace("Ana", "Eva"));
            Check(store.Load().Active.name == "Ana" && store.Recovered, "Checksum detects changed payload");
            File.WriteAllText(savePath, "bad"); File.WriteAllText(savePath + ".bak", "bad");
            Check(store.Load().players.Count == 0 && store.Unreadable, "Both bad copies report failure");
            store.Save(archive); Check(store.Load().players.Count == 3, "Session can save after both copies fail");
            // A crash before replacement leaves .tmp ignored and the committed primary unchanged.
            File.WriteAllText(savePath + ".tmp", "partial"); Check(store.Load().players.Count == 3, "Interrupted write ignored");
            archive.players[0].progress.schema = 99;
            Throws(() => store.Save(archive), "Invalid write rejected");
            Check(store.Load().players.Count == 3, "Failed save preserves committed archive");
        }
        finally { Directory.Delete(folder, true); }
        var memory = new MemorySequence(new[] { 2, 0, 3, 1 });
        Check(memory.Press(2) && !memory.Press(1) && memory.Position == 0, "Memory resets on error");
        foreach(int symbol in new[] { 2, 0, 3, 1 }) Check(memory.Press(symbol), "Memory sequence");
        Check(memory.Complete && !memory.Press(1), "Memory cannot finish twice");
        Console.WriteLine($"PASS: {checks} domain and persistence checks (real C# files; no Unity APIs).");
    }
}
