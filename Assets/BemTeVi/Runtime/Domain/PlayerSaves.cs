using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BemTeVi
{
    [Serializable] public sealed class PlayerProfile
    {
        public string id, name, updatedUtc;
        public string stageId = "quarto";
        public SaveData progress = new SaveData();
        public List<PuzzleSave> puzzles = new List<PuzzleSave>();
    }
    [Serializable] public sealed class PuzzleSave
    {
        public string mapId;
        public int round;
        public int[] digits = new int[3];
        public int[] assignments = { -1, -1, -1 };
    }
    [Serializable] public sealed class PlayerArchive
    {
        public int schema = 1;
        public string activeId;
        public List<PlayerProfile> players = new List<PlayerProfile>();
        public PlayerProfile Active => players.FirstOrDefault(p => p.id == activeId);
        public PlayerProfile Create(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0 || name.Length > 24 || name.Any(char.IsControl))
                throw new ArgumentException("Escreva um nome de 1 a 24 letras.");
            if (players.Any(p => string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Esse nome já tem uma aventura. Escolha-o na lista.");
            var player = new PlayerProfile { id = Guid.NewGuid().ToString("N"), name = name };
            players.Add(player); activeId = player.id; return player;
        }
        public void Validate(Catalog catalog)
        {
            if (schema != 1 || players == null) throw new ArgumentException("Arquivo de jogadores incompatível.");
            var ids = new HashSet<string>();
            var stages = new HashSet<string>(catalog.maps.SelectMany(m => m.rooms.Concat(new[] { m.junction, m.challenge })).Select(r => r.id)) { "quarto" };
            foreach (var player in players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.id) || !ids.Add(player.id) ||
                    string.IsNullOrWhiteSpace(player.name) || player.progress == null)
                    throw new ArgumentException("Perfil inválido.");
                player.progress = new Progress(catalog, player.progress, false).Data;
                if (!stages.Contains(player.stageId ?? "")) player.stageId = "quarto";
                player.puzzles = (player.puzzles ?? new List<PuzzleSave>()).Where(p => p != null &&
                    catalog.maps.Any(m => m.id == p.mapId) && p.round >= 0 && p.round < Progress.RoundCount &&
                    p.digits != null && p.digits.Length == 3 && p.digits.All(d => d >= 0 && d <= 9) &&
                    p.assignments != null && p.assignments.Length == 3 && p.assignments.All(a => a >= -1 && a < 3))
                    .GroupBy(p => p.mapId + "/" + p.round).Select(g => g.First()).ToList();
                foreach (var puzzle in player.puzzles)
                {
                    var map = catalog.maps.First(m => m.id == puzzle.mapId);
                    var solution = map.puzzleRounds[puzzle.round].solution;
                    for (int i = 0; i < 3; i++)
                        if (map.puzzle == "matching" && puzzle.assignments[i] != solution[i]) puzzle.assignments[i] = -1;
                }
            }
            if (Active == null) activeId = players.FirstOrDefault()?.id;
        }
    }

    [Serializable] public sealed class SaveEnvelope
    {
        public int format = 1;
        public string payload, checksum;
    }

    // JSON encoding is injected so the real disk/recovery behavior can be tested without Unity.
    public sealed class PlayerSaveStore
    {
        readonly string path;
        readonly Func<PlayerArchive, string> encode;
        readonly Func<string, PlayerArchive> decode;
        readonly Func<SaveEnvelope, string> wrap;
        readonly Func<string, SaveEnvelope> unwrap;
        readonly Action<PlayerArchive> validate;
        public bool Recovered { get; private set; }
        public bool Unreadable { get; private set; }
        public PlayerSaveStore(string path, Func<PlayerArchive, string> encode, Func<string, PlayerArchive> decode,
            Func<SaveEnvelope, string> wrap, Func<string, SaveEnvelope> unwrap, Action<PlayerArchive> validate)
        {
            this.path = path; this.encode = encode; this.decode = decode;
            this.wrap = wrap; this.unwrap = unwrap; this.validate = validate;
        }
        static string Hash(string text)
        {
            using (var hash = SHA256.Create())
                return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        PlayerArchive Read(string file)
        {
            var envelope = unwrap(File.ReadAllText(file, Encoding.UTF8));
            if (envelope == null || envelope.format != 1 || envelope.payload == null || envelope.checksum != Hash(envelope.payload))
                throw new InvalidDataException("Progresso incompleto.");
            var archive = decode(envelope.payload);
            if (archive == null) throw new InvalidDataException("Progresso vazio.");
            validate(archive); return archive;
        }
        bool TryRead(string file, out PlayerArchive archive)
        {
            try { archive = Read(file); return true; }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException || e is FormatException)
            { archive = null; return false; }
        }
        public PlayerArchive Load()
        {
            Recovered = Unreadable = false;
            if (TryRead(path, out var archive)) return archive;
            if (TryRead(path + ".bak", out archive)) { Recovered = true; return archive; }
            Unreadable = File.Exists(path) || File.Exists(path + ".bak");
            return new PlayerArchive();
        }
        public void Save(PlayerArchive archive)
        {
            // Validate a copy: checking another profile must not change the active round in memory.
            string payload = encode(archive);
            var copy = decode(payload); validate(copy);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(wrap(new SaveEnvelope { payload = payload, checksum = Hash(payload) }));
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                Read(temp); // Never replace a good save with an incomplete write.
                if (File.Exists(path))
                {
                    bool good = TryRead(path, out _);
                    if (!good) File.Copy(path, path + ".corrupt-" + DateTime.UtcNow.Ticks, false);
                    File.Replace(temp, path, good ? path + ".bak" : null);
                }
                else File.Move(temp, path);
                if (!TryRead(path + ".bak", out _)) File.Copy(path, path + ".bak", true);
                Unreadable = Recovered = false;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }
}
