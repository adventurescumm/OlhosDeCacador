using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OlhosDeCacador
{
    public sealed partial class HunterGame
    {
        RoomData currentStage;
        RoomView challengeView;
        WorldHotspot hoveredSpot;
        string lastMessage = "";
        float stageViewportHeight = 900;
        Image feedbackPanel;
        void CreateFeedback()
        {
            feedbackPanel = ui.Panel(260, 20, 920, 76, new Color(1f, .96f, .86f, .93f));
            hoverText = ui.Label("", 276, 28, 888, 62, 23, HunterUI.Ink, false, TextAnchor.MiddleCenter);
            UpdateFeedbackVisibility();
        }
        void UpdateFeedbackVisibility()
        {
            if (feedbackPanel != null) feedbackPanel.gameObject.SetActive(hoverText != null && !string.IsNullOrEmpty(hoverText.text));
        }
        readonly List<WorldLabel> worldLabels = new List<WorldLabel>();
        sealed class WorldLabel { public RectTransform rect; public WorldHotspot spot; public float width; }

        void CloseChallenge()
        {
            challengeView?.Dispose(); challengeView = null;
        }
        static Vector3 Position(float[] coordinates, Vector3 fallback) => coordinates != null && coordinates.Length == 3
            ? new Vector3(coordinates[0], coordinates[1], coordinates[2]) : fallback;
        void UpdateStageCamera(bool advanceAnimation = true)
        {
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            float left = (Screen.width - 1440 * scale) * .5f, bottom = (Screen.height - 900 * scale) * .5f;
            const float top = 0;
            if (mode == Mode.Title || mode == Mode.Menu || mode == Mode.Junction) stageViewportHeight = 900;
            else if (mode == Mode.Explore || mode == Mode.Puzzle) stageViewportHeight = 760;
            float height = stageViewportHeight;
            worldCamera.rect = new Rect(left / Screen.width, (bottom + (900 - top - height) * scale) / Screen.height,
                1440 * scale / Screen.width, height * scale / Screen.height);
            Vector3 eye = viewingShelf ? new Vector3(0, 8, -13.6f) : Position(currentStage?.camera, new Vector3(0, 6.3f, -9.2f));
            Vector3 look = viewingShelf ? new Vector3(0, .4f, .5f) : Position(currentStage?.lookAt, new Vector3(0, 1, .4f));
            float fov = viewingShelf ? 18 : currentStage != null && currentStage.fieldOfView > 0 ? currentStage.fieldOfView : 36;
            if (HunterStage.Active != null)
            {
                eye = HunterStage.Active.cameraPosition; look = HunterStage.Active.lookAt; fov = HunterStage.Active.fieldOfView;
            }
            // Fixed close composition: no scroll zoom or automatic discovery pan.
            worldCamera.fieldOfView = fov;
            worldCamera.transform.position = eye; worldCamera.transform.LookAt(look);
            foreach (var label in worldLabels)
            {
                if (label.rect == null || label.spot == null || !label.rect.gameObject.activeInHierarchy) continue;
                // Label beneath the model: the original mesh remains available for pointing/clicking.
                var screen = worldCamera.WorldToScreenPoint(label.spot.transform.position);
                float x = (screen.x - left) / scale;
                float y = 900 - (screen.y - bottom) / scale + 12;
                label.rect.anchoredPosition = new Vector2(Mathf.Clamp(x - label.width * .5f, 12, 1428 - label.width), -Mathf.Clamp(y, 100, mode == Mode.Menu ? 724 : 642));
            }
        }
        void WorldButton(WorldHotspot spot, string text, Color color, float width, bool enabled = true)
        {
            var button = ui.Button(text, 0, 0, width, 36, () => Activate(spot), color, enabled, 25);
            worldLabels.Add(new WorldLabel { rect = button.GetComponent<RectTransform>(), spot = spot, width = width });
            UpdateStageCamera(false); // Avoid a frame with newly created labels stacked at the origin.
        }
        void ClearWorldHover()
        {
            SetHover(null);
            if (hoveredSpot != null) hoveredSpot.Hover(false);
            hoveredSpot = null;
        }
        void UpdateWorldInput()
        {
            if ((mode != Mode.Menu && mode != Mode.Junction && mode != Mode.Explore && mode != Mode.Puzzle) || transition > 0)
            { ClearWorldHover(); return; }
            var mouse = Mouse.current;
            if (mouse == null) { ClearWorldHover(); return; }
            var pointer = mouse.position.ReadValue();
            if (PointerOverUI(pointer) || !worldCamera.pixelRect.Contains(pointer)) { ClearWorldHover(); return; }
            HuntTarget target = null; WorldHotspot spot = null;
            if (Physics.Raycast(worldCamera.ScreenPointToRay(pointer), out var hit, 60))
            {
                if (mode == Mode.Explore) target = hit.collider.GetComponentInParent<HuntTarget>();
                spot = hit.collider.GetComponentInParent<WorldHotspot>();
            }
            SetHover(target);
            if (hoveredSpot != spot)
            {
                if (hoveredSpot != null) hoveredSpot.Hover(false);
                hoveredSpot = spot;
                if (spot != null) spot.Hover(true);
            }
            if (spot != null && hoverText != null && spot.Data.action != "inspect") hoverText.text = spot.Data.label;
            UpdateFeedbackVisibility();
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (target != null) Collect(target);
                else if (spot != null) Activate(spot);
                else if (mode == Mode.Explore && hit.collider != null) Tell("Esse brinquedo não está na charada deste cantinho.");
            }
        }
        string ChallengeInvitation() => "O mecanismo final está na última maquete. Siga as passagens!";
        void Activate(WorldHotspot spot)
        {
            if (spot == null) return;
            var data = spot.Data;
            if (mode == Mode.Menu && data.action == "world")
            {
                EnterMap(catalog.maps.First(m => m.id == data.destination)); return;
            }
            if (mode == Mode.Explore || mode == Mode.Junction || mode == Mode.Puzzle)
            {
                if (data.action == "exit") { FollowPassage(spot); return; }
                if (data.action == "gate") { UseGate(spot); return; }
            }
            if (data.action == "inspect" && (mode == Mode.Menu || mode == Mode.Explore || mode == Mode.Junction))
            {
                spot.React(); PlayObjectSound(data, false); Tell(data.label); return;
            }
            if (mode == Mode.Puzzle && data.action == "replay")
            { StopSignal(); signal = StartCoroutine(ShowSignal()); spot.React(); return; }
            if (mode != Mode.Puzzle || playingSignal) return;
            // Direct mesh clicks and the accessible labels share exactly the same action.
            switch (data.action)
            {
                case "digit":
                    digits[data.value] = (digits[data.value] + 1) % 10; spot.React(); Tap(); DrawPuzzle(); break;
                case "unlock":
                    if (PuzzleRules.Combination(digits, map.solution)) Win();
                    else { Play(wrong); spot.React(); puzzleMessage.text = "Ainda não abriu. Confira a ordem das três pistas."; }
                    break;
                case "signal": spot.Mark(.5f); spot.React(); PressSignal(data.value); break;
                case "supply":
                    if (!assignments.Contains(data.value)) { selectedSupply = data.value; spot.React(); Tap(); DrawPuzzle(); }
                    break;
                case "destination": Match(data.value); break;
            }
        }
    }
}
