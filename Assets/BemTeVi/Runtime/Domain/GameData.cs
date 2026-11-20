using System;
using System.Collections.Generic;
using System.Linq;

namespace BemTeVi
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
        [NonSerialized] public int currentRound;
        public PuzzleRound[] puzzleRounds;
        public IEnumerable<PropData> AllTargets => rooms.SelectMany(r => r.objects).Where(p => !string.IsNullOrEmpty(p.id));
        public IEnumerable<PropData> Targets => AllTargets.Where(p => p.round == currentRound);
        public int[] Solution => puzzleRounds != null ? puzzleRounds[currentRound].solution : solution;
        public string[] Clues => puzzleRounds != null ? puzzleRounds[currentRound].clues : clues;
        public string PuzzleIntro => puzzleRounds != null ? puzzleRounds[currentRound].intro : puzzleTitle;
    }
    [Serializable] public sealed class PuzzleRound { public int[] solution; public string[] clues; public string intro; }
    [Serializable] public sealed class RoomData
    {
        public string id, name, caption, riddle;
        public string[] riddles;
        [NonSerialized] public int currentRound;
        public string Riddle => riddles != null ? riddles[currentRound] : riddle;
        public IEnumerable<PropData> Targets => objects.Where(p => !string.IsNullOrEmpty(p.id) && p.round == currentRound);
        public string layout, floor, wall, color;
        public float width = 12, depth = 8;
        public float[] camera, lookAt;
        public float fieldOfView;
        public PropData[] objects;
        public PrimitiveData[] geometry = Array.Empty<PrimitiveData>();
    }
    [Serializable] public sealed class GateData
    {
        public string id, title, tokenId, roomId;
        public int sourceRoom;
    }
    [Serializable] public sealed class PropData
    {
        public string asset, id, label, clue, sound;
        public string action, destination, reaction;
        public int value, round;
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
        public const int RoundCount = 3;
        readonly Catalog catalog;
        public SaveData Data { get; private set; }
        static string Key(MapData map, int round) => map.id + "/r" + (round + 1);
        public Progress(Catalog catalog, SaveData data = null, bool activateRounds = true)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Data = data ?? new SaveData();
            if (Data.schema != 1 || Data.found == null || Data.solved == null)
                throw new ArgumentException("Formato de progresso inválido.");
            var known = new HashSet<string>(catalog.maps.SelectMany(m => m.AllTargets).Select(p => p.id));
            Data.found = Data.found.Where(known.Contains).Distinct().ToList();
            var valid = new List<string>();
            foreach (var map in catalog.maps)
            {
                int completed = 0;
                for (int round = 0; round < RoundCount; round++)
                {
                    bool solved = Data.solved.Contains(Key(map, round)) || (round == 0 && Data.solved.Contains(map.id));
                    if (!solved || !map.AllTargets.Where(p => p.round == round).All(p => Has(p.id))) break;
                    valid.Add(Key(map, round)); completed++;
                }
                int activeRound = Math.Min(completed, RoundCount - 1);
                if (activateRounds) SetRound(map, activeRound);
                // A save cannot reveal later-round objects before that round is unlocked.
                var future = new HashSet<string>(map.AllTargets.Where(p => p.round > activeRound).Select(p => p.id));
                Data.found.RemoveAll(future.Contains);
            }
            Data.solved = valid;
            Data.opened = (Data.opened ?? new List<string>()).Where(id => catalog.maps.Any(m =>
                (m.gates ?? Array.Empty<GateData>()).Any(g => g.id == id && Has(g.tokenId)))).Distinct().ToList();
        }
        void SetRound(MapData map, int round)
        {
            map.currentRound = round;
            foreach (var room in map.rooms) room.currentRound = round;
        }
        public bool Has(string id) => Data.found.Contains(id);
        public bool Active(MapData map, PropData prop) => !string.IsNullOrEmpty(prop.id) && prop.round == map.currentRound;
        public bool Solved(MapData map) => Enumerable.Range(0, RoundCount).All(r => Data.solved.Contains(Key(map, r)));
        public int CompletedRounds(MapData map) => Enumerable.Range(0, RoundCount).Count(r => Data.solved.Contains(Key(map, r)));
        public int Count(MapData map) => map.Targets.Count(p => Has(p.id));
        public bool RoomReady(RoomData room) => room.Targets.All(p => Has(p.id));
        public bool Opened(string gate) => Data.opened.Contains(gate);
        public bool CanVisit(MapData map, string roomId) => map.rooms.Any(r => r.id == roomId);
        public bool CanTraverse(MapData map, RoomData from, string destination)
        {
            if (from == null || !from.objects.Any(p => p.action == "exit" && p.destination == destination)) return false;
            if (destination == "quarto") return from == map.junction;
            return destination == map.junction.id || destination == map.challenge.id || CanVisit(map, destination);
        }
        public bool Use(MapData map, string gateId, string tokenId)
        {
            var gate = (map.gates ?? Array.Empty<GateData>()).FirstOrDefault(g => g.id == gateId);
            if (gate == null || Opened(gateId) || gate.tokenId != tokenId || !Has(tokenId)) return false;
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
            if (!Ready(map) || Data.solved.Contains(Key(map, map.currentRound))) return false;
            Data.solved.Add(Key(map, map.currentRound));
            if (map.currentRound + 1 < RoundCount) SetRound(map, map.currentRound + 1);
            return true;
        }
        public void ResetMap(MapData map)
        {
            var ids = new HashSet<string>(map.AllTargets.Select(p => p.id));
            Data.found.RemoveAll(ids.Contains);
            Data.solved.RemoveAll(id => Enumerable.Range(0, RoundCount).Any(r => id == Key(map, r)));
            var gates = new HashSet<string>((map.gates ?? Array.Empty<GateData>()).Select(g => g.id));
            Data.opened.RemoveAll(gates.Contains); SetRound(map, 0);
        }
        public void ResetAll()
        {
            Data = new SaveData();
            foreach (var map in catalog.maps) SetRound(map, 0);
        }
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
