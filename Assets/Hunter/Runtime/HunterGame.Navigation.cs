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
            roomIndex = -1; currentStage = map.junction; 
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
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, HunterUI.Plum, true, 21);
            CreateFeedback();
        }
        void FollowPassage(WorldHotspot spot)
        {
            string destination = spot.Data.destination;
            if (!progress.CanTraverse(map, currentStage, destination))
            {
                spot.React(); Tell(destination == map.challenge.id ? "O final espera pelos dezoito tesouros e pelas duas passagens consertadas." : "Esta passagem ainda não funciona. Investigue o mecanismo na maquete anterior."); return;
            }
            PlayStageSound("passage");
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
            Save(); PlayStageSound("mechanism"); room.ApplyMechanisms(progress, !reduceMotion);
            Tell("A peça encaixou! Observe a maquete: a passagem para " + map.rooms.First(r => r.id == gate.roomId).name + " está aberta.");
        }
    }
}
