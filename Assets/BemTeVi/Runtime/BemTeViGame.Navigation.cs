using System;
using System.Linq;
using UnityEngine;

namespace BemTeVi
{
    public sealed partial class BemTeViGame
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
                RememberStage();
            }
            catch (Exception e) { ShowError(e); }
        }
        void DrawJunction()
        {
            mode = Mode.Junction; ui.Clear(); worldLabels.Clear(); listenButton = null;
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, BemTeViUI.Plum, true, 21);
            CreateFeedback();
        }
        void FollowPassage(WorldHotspot spot)
        {
            string destination = spot.Data.destination;
            if (!progress.CanTraverse(map, currentStage, destination))
            {
                spot.React(); Tell(destination == map.challenge.id ? "Siga uma das passagens iluminadas." : "Esta passagem ainda não funciona. Investigue o mecanismo na maquete anterior."); return;
            }
            PlayStageSound("passage");
            if (destination == "quarto") { ShowMenu(); return; }
            if (destination == map.challenge.id) { OpenPuzzle(); return; }
            if (destination == map.junction.id) { ShowJunction(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == destination);
            LoadRoom();
        }
        void UseGate(WorldHotspot spot)
        {
            var gate = map.gates.First(g => g.id == spot.Data.destination);
            if (progress.Opened(gate.id)) { spot.React(); return; }
            if (!progress.Has(gate.tokenId)) { spot.React(); PlayObjectSound(spot.Data, false); return; }
            if (!progress.Use(map, gate.id, gate.tokenId)) return;
            Save(); PlayStageSound("mechanism"); room.ApplyMechanisms(progress, !reduceMotion);

        }
    }
}
