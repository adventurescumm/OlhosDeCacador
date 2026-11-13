using System;
using System.Collections.Generic;
using System.Linq;

namespace OlhosDeCacador
{
    [Serializable] public sealed class Catalog { public MapData[] maps; public RoomData hub; }
    [Serializable] public sealed class MapData
    {
        public string id, title, subtitle, story, color, floor, wall, puzzle, puzzleTitle;
        public string[] clues;
        public int[] solution;
        public RoomData[] rooms;
        public RoomData challenge, junction;
        public GateData[] gates = Array.Empty<GateData>();
        public IEnumerable<PropData> Targets => rooms.SelectMany(r => r.objects).Where(p => !string.IsNullOrEmpty(p.id));
    }
    [Serializable] public sealed class RoomData
    {
        public string id, name, caption, riddle;
        public string layout, floor, wall, color;
        public float width = 12, depth = 8;
        public float[] camera, lookAt;
        public float fieldOfView;
        public PropData[] objects;
        public PrimitiveData[] geometry = Array.Empty<PrimitiveData>();
    }
    [Serializable] public sealed class GateData
    {
        public string id, title, tokenId, roomId, hint;
        public int sourceRoom;
    }
    [Serializable] public sealed class PropData
    {
        public string asset, id, label, clue, hint;
        public string action, destination, reaction;
        public int value;
        public float x, y, z, size, rotation;
        public float tiltX, tiltZ;
        public string gate, landmark, showAfter;
        public float[] openedOffset, openedRotation;
    }
    // Only primitive room/plinth shapes; artwork always comes from downloaded models.
    [Serializable] public sealed class PrimitiveData
    {
        public string name, shape, color;
        public float x, y, z, width, height, depth, rotation;
        public bool decorative;
    }
    [Serializable] public sealed class SaveData
    {
        public int schema = 1;
        public List<string> found = new List<string>();
        public List<string> solved = new List<string>();
        public List<string> opened = new List<string>();
    }

    // Pure domain logic: no engine, UI, disk or physics dependencies.
    public sealed class Progress
    {
        readonly Catalog catalog;
        public SaveData Data { get; private set; }
        public Progress(Catalog catalog, SaveData data = null)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Data = data ?? new SaveData();
            if (Data.schema != 1 || Data.found == null || Data.solved == null)
                throw new ArgumentException("Formato de progresso inválido.");
            var known = new HashSet<string>(catalog.maps.SelectMany(m => m.Targets).Select(p => p.id));
            Data.found = Data.found.Where(known.Contains).Distinct().ToList();
            // Older saves have no opened field. Retain found IDs, then validate the new passages.
            Data.opened = (Data.opened ?? new List<string>()).Where(id => catalog.maps.Any(m =>
                (m.gates ?? Array.Empty<GateData>()).Any(g => g.id == id && RoomReady(m.rooms[g.sourceRoom]) && Has(g.tokenId)))).Distinct().ToList();
            Data.solved = Data.solved.Where(id => catalog.maps.Any(m => m.id == id && Ready(m))).Distinct().ToList();
        }
        public bool Has(string id) => Data.found.Contains(id);
        public bool Solved(MapData map) => Data.solved.Contains(map.id);
        public int Count(MapData map) => map.Targets.Count(p => Has(p.id));
        public bool RoomReady(RoomData room) => room.objects.Where(p => !string.IsNullOrEmpty(p.id)).All(p => Has(p.id));
        public bool Opened(string gate) => Data.opened.Contains(gate);
        public bool CanVisit(MapData map, string roomId) => map.rooms.Any(r => r.id == roomId) &&
            !(map.gates ?? Array.Empty<GateData>()).Any(g => g.roomId == roomId && !Opened(g.id));
        public bool CanTraverse(MapData map, RoomData from, string destination)
        {
            if (from == null || !from.objects.Any(p => p.action == "exit" && p.destination == destination)) return false;
            if (destination == "quarto") return from == map.junction;
            if (destination == map.junction.id) return true;
            if (destination == map.challenge.id) return Ready(map) && map.gates.All(g => Opened(g.id));
            return CanVisit(map, destination);
        }
        public bool Use(MapData map, string gateId, string tokenId)
        {
            var gate = (map.gates ?? Array.Empty<GateData>()).FirstOrDefault(g => g.id == gateId);
            if (gate == null || Opened(gateId) || gate.tokenId != tokenId || !Has(tokenId) || !RoomReady(map.rooms[gate.sourceRoom])) return false;
            Data.opened.Add(gateId); return true;
        }
        public bool Ready(MapData map) => map.Targets.All(p => Has(p.id));
        public bool Finished => catalog.maps.All(Solved);
        public bool Collect(string id)
        {
            if (Has(id) || !catalog.maps.Any(m => m.Targets.Any(p => p.id == id))) return false;
            Data.found.Add(id); return true;
        }
        public bool Complete(MapData map)
        {
            if (!Ready(map) || Solved(map)) return false;
            Data.solved.Add(map.id); return true;
        }
        public void ResetMap(MapData map)
        {
            var ids = new HashSet<string>(map.Targets.Select(p => p.id));
            Data.found.RemoveAll(ids.Contains); Data.solved.Remove(map.id);
            var gates = new HashSet<string>((map.gates ?? Array.Empty<GateData>()).Select(g => g.id));
            Data.opened.RemoveAll(gates.Contains);
        }
        public void ResetAll() => Data = new SaveData();
    }

    public static class PuzzleRules
    {
        public static bool Combination(int[] input, int[] expected) => input != null && expected != null && input.SequenceEqual(expected);
        public static bool Match(int supply, int destination, int[] solution) => destination >= 0 && destination < solution.Length && supply == solution[destination];
    }

    public sealed class MemorySequence
    {
        readonly int[] expected;
        public int Position { get; private set; }
        public bool Complete => Position == expected.Length;
        public MemorySequence(int[] expected)
        {
            if (expected == null || expected.Length == 0) throw new ArgumentException("Sequência vazia.");
            this.expected = (int[])expected.Clone();
        }
        public void Reset() => Position = 0;
        public bool Press(int value)
        {
            if (Complete) return false;
            if (expected[Position] != value) { Reset(); return false; }
            Position++; return true;
        }
    }
}
