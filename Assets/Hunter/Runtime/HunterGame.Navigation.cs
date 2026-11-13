using System;
using System.Linq;
using UnityEngine;

namespace OlhosDeCacador
{
    public sealed partial class HunterGame
    {
        string selectedPiece;

        void ShowJunction()
        {
            if (map == null) { ShowMenu(); return; }
            if (RouteTo(map.junction.id, ShowJunction, room != null)) return;
            StopSignal(); StopNarration(); ClearWorldHover(); CloseChallenge();
            room?.Dispose(); shelf?.Dispose(); shelf = null; viewingShelf = false;
            roomIndex = -1; currentStage = map.junction; focusUntil = 0; focusBlend = 0;
            selectedPiece = null; messageUntil = 0; lastMessage = "";
            try
            {
                room = new RoomView(map, currentStage, progress, reduceMotion);
                worldCamera.backgroundColor = RoomView.Hex(currentStage.wall);
                transition = reduceMotion ? 0 : .35f;
                DrawJunction();
            }
            catch (Exception e) { ShowError(e); }
        }

        void DrawJunction()
        {
            mode = Mode.Junction; ui.Clear(); worldLabels.Clear(); listenButton = null;
            var accent = RoomView.Hex(currentStage.color);
            ui.Panel(0, 0, 1440, 56, accent);
            ui.Label(currentStage.name, 24, 8, 710, 40, 29, Color.white, true);
            ui.Label(progress.Count(map) + " / " + map.Targets.Count() + " descobertas", 746, 16, 260, 32, 20, HunterUI.Paper);
            ui.Button("Quarto", 1030, 8, 180, 40, ShowMenu, HunterUI.Teal, true, 19);
            ui.Button("Pausa", 1230, 8, 186, 40, ShowPause, HunterUI.Plum, true, 19);
            ui.Panel(0, 730, 1440, 170, accent); ui.Sheet(12, 738, 1416, 150);
            ui.Label("PEÇAS DA AVENTURA", 30, 749, 620, 32, 25, accent, true);
            ui.Label("Complete um cantinho, escolha sua peça e clique no brinquedo que precisa dela.", 30, 785, 1010, 30, 19, HunterUI.Ink);
            for (int i = 0; i < map.gates.Length; i++)
            {
                var gate = map.gates[i]; var target = map.Targets.First(p => p.id == gate.tokenId);
                bool placed = progress.Opened(gate.id), foundPiece = progress.Has(gate.tokenId);
                bool ready = !placed && foundPiece && progress.RoomReady(map.rooms[gate.sourceRoom]);
                string label = placed ? "Colocada: " + target.label : (foundPiece ? "Usar: " : "Procurar: ") + target.label;
                ui.Button(label, 30 + i * 495, 828, 478, 46,
                    () => { selectedPiece = gate.tokenId; Tap(); DrawJunction(); Tell("Peça escolhida. Clique no brinquedo do cenário que precisa dela."); },
                    selectedPiece == gate.tokenId ? HunterUI.Plum : i == 0 ? HunterUI.Teal : RoomView.Hex("#B74E6A"), ready, 19);
            }
            ui.Button("Desafio final", 1052, 806, 340, 66, OpenPuzzle, RoomView.Hex("#73549E"), progress.Ready(map) && !progress.Solved(map), 25);
            ui.Panel(280, 684, 880, 34, new Color(.08f, .10f, .21f, .82f));
            hoverText = ui.Label("Os brinquedos grandes guardam caminhos para depois. Investigue!", 292, 686, 856, 30, 18, Color.white, false, TextAnchor.MiddleCenter);
            foreach (var spot in room.Hotspots)
            {
                if (spot.Data.action == "travel")
                {
                    bool open = progress.CanVisit(map, spot.Data.destination);
                    WorldButton(spot, (open ? "Ir: " : "Fechado: ") + spot.Data.label, open ? HunterUI.Teal : RoomView.Hex("#805D78"), 290);
                }
                else if (spot.Data.action == "gate")
                    WorldButton(spot, progress.Opened(spot.Data.destination) ? "Peça no lugar!" : spot.Data.label, RoomView.Hex("#AF4A68"), 260);
                else if (spot.Data.action == "puzzle") WorldButton(spot, spot.Data.label, HunterUI.Plum, 230);
            }
        }

        void UseGate(WorldHotspot spot)
        {
            var gate = map.gates.First(g => g.id == spot.Data.destination);
            if (progress.Opened(gate.id)) { spot.React(); Tell("Este brinquedo já está pronto! O caminho seguinte está aberto."); return; }
            if (string.IsNullOrEmpty(selectedPiece))
            {
                spot.React(); Tell(gate.hint + " Primeiro complete os seis objetos do cantinho anterior."); return;
            }
            if (!progress.Use(map, gate.id, selectedPiece))
            {
                Play(wrong); Tell("Essa peça ainda não resolve aqui. " + gate.hint); return;
            }
            Save(); Play(found);
            RouteTo(map.junction.id, () =>
            {
                ShowJunction();
                var placed = room.Hotspots.First(h => h.Data.action == "gate" && h.Data.destination == gate.id);
                placed.React(); Tell("Deu certo! " + map.rooms.First(r => r.id == gate.roomId).name + " está aberto. A peça ficou no cenário.");
            }, true);
        }
    }
}
