using System;
using System.Linq;
using UnityEngine;

namespace BemTeVi
{
    public sealed partial class BemTeViGame
    {
        // Each world has a physical map: every search and the challenge has an open route.
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
            ui.Label(currentStage.name, 28, 20, 850, 42, 27, BemTeViUI.Paper, true);
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, BemTeViUI.Plum, true, 21);
            DrawPassages(room);
            CreateFeedback();
        }
        void DrawPassages(RoomView view)
        {
            if (view == null) return;
            // Destination names are physical plaques on the models, as in the reference halls.
            if (currentStage != map.junction)
            {
                var mapExit = view.Hotspots.FirstOrDefault(h => h.Data.action == "exit" && h.Data.destination == map.junction.id);
                if (mapExit != null)
                    ui.Button("Mapa", 1176, 18, 120, 42, () => FollowPassage(mapExit), BemTeViUI.Teal, true, 21);
            }
        }
        void FollowPassage(WorldHotspot spot)
        {
            string destination = spot.Data.destination;
            if (!progress.CanTraverse(map, currentStage, destination))
            {
                spot.React(); return;
            }
            PlayStageSound("passage");
            if (destination == "quarto") { ShowMenu(); return; }
            if (destination == map.challenge.id) { OpenPuzzle(); return; }
            if (destination == map.junction.id) { ShowJunction(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == destination);
            LoadRoom();
        }
    }
}
