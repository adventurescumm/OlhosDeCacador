using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BemTeVi
{
    public sealed partial class BemTeViGame
    {
        PlayerArchive players;
        PlayerProfile activePlayer;
        PlayerSaveStore saveStore;
        int playerPage;

        void LoadPlayers()
        {
            saveStore = new PlayerSaveStore(Path.Combine(Application.persistentDataPath, "Saves", "players.json"),
                a => JsonUtility.ToJson(a), raw => JsonUtility.FromJson<PlayerArchive>(raw),
                e => JsonUtility.ToJson(e), raw => JsonUtility.FromJson<SaveEnvelope>(raw), a => a.Validate(catalog));
            players = saveStore.Load();
            if (saveStore.Recovered) saveWarning = "Seu caderno foi recuperado da cópia de segurança.";
            if (saveStore.Unreadable) saveWarning = "Não foi possível abrir os cadernos anteriores. Os arquivos foram preservados.";
            if (players.players.Count == 0 && !saveStore.Unreadable) ImportLegacyProgress();
            activePlayer = players.Active;
            progress = new Progress(catalog, activePlayer?.progress);
        }
        void ImportLegacyProgress()
        {
            var candidates = new[] { SaveKey, SaveKey + ".backup", "OlhosDeCacador.v1", "OlhosDeCacador.v1.backup" }
                .Select(key => PlayerPrefs.GetString(key, "")).Concat(LegacyProgressReader.ReadWindowsSaves());
            foreach (string raw in candidates)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                try
                {
                    var data = JsonUtility.FromJson<SaveData>(raw);
                    if (data == null) continue;
                    var recovered = new Progress(catalog, data, false);
                    var player = players.Create("Minha aventura"); player.progress = recovered.Data;
                    saveStore.Save(players); return; // Original PlayerPrefs keys remain untouched.
                }
                catch (Exception e) { Debug.LogWarning("Não foi possível migrar o progresso: " + e.Message); }
            }
        }
        void Save()
        {
            try
            {
                if (activePlayer != null && progress != null)
                {
                    activePlayer.progress = progress.Data;
                    activePlayer.updatedUtc = DateTime.UtcNow.ToString("o");
                    saveStore.Save(players);
                }
                PlayerPrefs.SetInt(SaveKey + ".mute", muted ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".motion", reduceMotion ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".autoVoice", autoNarration ? 1 : 0);
                PlayerPrefs.SetFloat(SaveKey + ".effectsVolume", effectsVolume);
                PlayerPrefs.SetFloat(SaveKey + ".voiceVolume", narrationVolume);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Falha ao guardar o caderno: " + e.Message);
                saveWarning = "Não foi possível salvar neste computador. Você pode continuar nesta sessão.";
                if (mode == Mode.Explore || mode == Mode.Junction || mode == Mode.Menu) Tell(saveWarning);
            }
        }
        void RememberStage()
        {
            if (activePlayer == null || currentStage == null) return;
            activePlayer.stageId = currentStage.id; Save();
        }
        void ShowPlayers()
        {
            StopNarration(); StopSignal(); ClearWorldHover(); mode = Mode.Profiles;
            ui.Modal("De quem é a aventura?", "Escolha seu nome para continuar. Seu caderno é guardado automaticamente.");
            playerPage = Mathf.Clamp(playerPage, 0, Math.Max(0, (players.players.Count - 1) / 4));
            int row = 0;
            foreach (var player in players.players.Skip(playerPage * 4).Take(4))
            {
                string label = player.name + "  •  " + player.progress.found.Count + " descobertas  •  " + player.progress.solved.Count + "/9 selos";
                ui.Button(label, 242, 326 + row++ * 66, 956, 54, () => ResumePlayer(player), BemTeViUI.Teal, true, 24);
            }
            if (players.players.Count == 0) ui.Label("Seu primeiro caderno começa aqui.", 242, 360, 956, 66, 29, BemTeViUI.Ink, false, TextAnchor.MiddleCenter);
            ui.Button("Anterior", 242, 600, 220, 46, () => { playerPage--; ShowPlayers(); }, BemTeViUI.Plum, playerPage > 0);
            ui.Label((playerPage + 1) + " / " + Math.Max(1, (players.players.Count + 3) / 4), 490, 606, 460, 36, 23, BemTeViUI.Ink, false, TextAnchor.MiddleCenter);
            ui.Button("Próxima", 978, 600, 220, 46, () => { playerPage++; ShowPlayers(); }, BemTeViUI.Plum, (playerPage + 1) * 4 < players.players.Count);
            ui.Button("Voltar", 242, 676, 468, 60, ShowTitle, BemTeViUI.Plum);
            ui.Button("Novo jogador", 730, 676, 468, 60, NewPlayer, BemTeViUI.Teal);
            if (!string.IsNullOrEmpty(saveWarning)) ui.Label(saveWarning, 242, 750, 956, 48, 18, BemTeViUI.Ink);
        }
        void NewPlayer()
        {
            mode = Mode.Dialog; dialogBack = ShowPlayers;
            ui.Modal("Seu caderno de aventuras", "Como você quer ser chamado?");
            var input = ui.NameInput(242, 354, 956, 76);
            var error = ui.Label("", 242, 452, 956, 88, 24, BemTeViUI.Plum);
            ui.Button("Voltar", 242, 674, 468, 62, ShowPlayers, BemTeViUI.Plum);
            ui.Button("Começar", 730, 674, 468, 62, () =>
            {
                try { ResumePlayer(players.Create(input.text)); }
                catch (ArgumentException e) { error.text = e.Message; }
            });
            input.ActivateInputField();
        }
        void ResumePlayer(PlayerProfile player)
        {
            StopNarration(); activePlayer = player; players.activeId = player.id;
            progress = new Progress(catalog, player.progress);
            string id = player.stageId; Save();
            map = catalog.maps.FirstOrDefault(m => m.junction.id == id || m.challenge.id == id || m.rooms.Any(r => r.id == id));
            if (map == null) { ShowMenu(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == id);
            if (roomIndex >= 0) LoadRoom();
            else if (id == map.challenge.id) { roomIndex = map.rooms.Length - 1; OpenPuzzle(); }
            else ShowJunction();
        }
        void SavePuzzle()
        {
            if (activePlayer == null || map == null) return;
            activePlayer.puzzles.RemoveAll(p => p.mapId == map.id && p.round == map.currentRound);
            activePlayer.puzzles.Add(new PuzzleSave { mapId = map.id, round = map.currentRound,
                digits = (int[])digits.Clone(), assignments = (int[])assignments.Clone() });
            Save();
        }
        void RestorePuzzle()
        {
            var saved = activePlayer?.puzzles.FirstOrDefault(p => p.mapId == map.id && p.round == map.currentRound);
            if (saved == null) return;
            digits = (int[])saved.digits.Clone(); assignments = (int[])saved.assignments.Clone();
            if (map.puzzle != "matching") return;
            for (int i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] < 0) continue;
                var item = challengeView.Hotspots.First(h => h.Data.action == "supply" && h.Data.value == assignments[i]);
                var place = challengeView.Hotspots.First(h => h.Data.action == "destination" && h.Data.value == i);
                item.PlaceAt(place.transform.localPosition + new Vector3(.7f, 0, -.6f));
            }
        }
    }
}
