using System;
using System.Linq;
using UnityEngine;

namespace OlhosDeCacador
{
    public sealed partial class HunterGame
    {
        // This is a physical arrival scene, not a destination selector.
        void ShowJunction()
        {
            if (map == null) { ShowMenu(); return; }
            if (RouteTo(map.junction.id, ShowJunction, room != null)) return;
            StopSignal(); StopNarration(); ClearWorldHover(); CloseChallenge();
            room?.Dispose(); shelf?.Dispose(); shelf = null; viewingShelf = false;
            roomIndex = -1; currentStage = map.junction; ResetMagnifier(); focusUntil = 0; focusBlend = 0;
            messageUntil = 0; lastMessage = "";
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
            ui.Label(currentStage.name, 24, 8, 720, 40, 29, Color.white, true);
            ui.Label(progress.Count(map) + " / 18 descobertas", 830, 14, 340, 32, 21, HunterUI.Paper);
            ui.Button("Pausa", 1230, 8, 186, 40, ShowPause, HunterUI.Plum, true, 19);
            ui.Panel(0, 810, 1440, 90, HunterUI.Paper);
            ui.Label("Cada passagem faz parte deste pequeno mundo.", 30, 818, 1380, 30, 25, accent, true, TextAnchor.MiddleCenter);
            hoverText = ui.Label("Clique nas portas, trilhos e pontes. A roda do mouse aproxima.", 50, 855, 1340, 32, 20, HunterUI.Ink, false, TextAnchor.MiddleCenter);
        }
        void FollowPassage(WorldHotspot spot)
        {
            string destination = spot.Data.destination;
            if (!progress.CanTraverse(map, currentStage, destination))
            {
                spot.React(); Tell(destination == map.challenge.id ? "O final espera pelos dezoito tesouros e pelas duas passagens consertadas." : "Esta passagem ainda não funciona. Investigue o mecanismo na maquete anterior."); return;
            }
            Tap();
            if (destination == "quarto") { ShowMenu(); return; }
            if (destination == map.challenge.id) { OpenPuzzle(); return; }
            if (destination == map.junction.id) { ShowJunction(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == destination);
            selectedTarget = null; LoadRoom();
        }
        void UseGate(WorldHotspot spot)
        {
            var gate = map.gates.First(g => g.id == spot.Data.destination);
            if (progress.Opened(gate.id)) { spot.React(); Tell("Está funcionando! A passagem ao lado leva a " + map.rooms.First(r => r.id == gate.roomId).name + "."); return; }
            if (!progress.Has(gate.tokenId)) { spot.React(); Tell(gate.hint); return; }
            if (!progress.RoomReady(map.rooms[gate.sourceRoom])) { Tell("Você já encontrou a peça. Complete os seis objetos da charada antes de montá-la."); return; }
            if (!progress.Use(map, gate.id, gate.tokenId)) return;
            Save(); Play(found); room.ApplyMechanisms(progress, !reduceMotion);
            Tell("A peça encaixou! Observe a maquete: a passagem para " + map.rooms.First(r => r.id == gate.roomId).name + " está aberta.");
        }
    }
}
