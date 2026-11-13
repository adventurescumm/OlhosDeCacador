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
        Vector3 focusPoint;
        float focusUntil, focusBlend;
        readonly List<WorldLabel> worldLabels = new List<WorldLabel>();
        sealed class WorldLabel { public RectTransform rect; public WorldHotspot spot; public float width; }

        void CloseChallenge()
        {
            challengeView?.Dispose(); challengeView = null;
        }
        void FocusOn(Vector3 position, float seconds)
        {
            if (reduceMotion) return;
            focusPoint = position + Vector3.up * .35f; focusUntil = Time.unscaledTime + seconds;
        }
        static Vector3 Position(float[] coordinates, Vector3 fallback) => coordinates != null && coordinates.Length == 3
            ? new Vector3(coordinates[0], coordinates[1], coordinates[2]) : fallback;
        void UpdateStageCamera(bool advanceAnimation = true)
        {
            float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            float left = (Screen.width - 1440 * scale) * .5f, bottom = (Screen.height - 900 * scale) * .5f;
            float top = viewingShelf ? 300 : 56, height = viewingShelf ? 340 : 704;
            worldCamera.rect = new Rect(left / Screen.width, (bottom + (900 - top - height) * scale) / Screen.height,
                1440 * scale / Screen.width, height * scale / Screen.height);
            Vector3 eye = viewingShelf ? new Vector3(0, 8, -13.6f) : Position(currentStage?.camera, new Vector3(0, 6.3f, -9.2f));
            Vector3 look = viewingShelf ? new Vector3(0, .4f, .5f) : Position(currentStage?.lookAt, new Vector3(0, 1, .4f));
            float fov = viewingShelf ? 18 : currentStage != null && currentStage.fieldOfView > 0 ? currentStage.fieldOfView : 36;
            bool focus = !reduceMotion && mode == Mode.Explore && Time.unscaledTime < focusUntil;
            if (advanceAnimation) focusBlend = reduceMotion ? 0 : Mathf.MoveTowards(focusBlend, focus ? 1 : 0, Time.unscaledDeltaTime * 3);
            // Fixed authored shots; a small focus on a discovery returns to the full composition.
            eye += (focusPoint - look) * (.22f * focusBlend);
            look = Vector3.Lerp(look, focusPoint, .22f * focusBlend);
            if (!reduceMotion && transition > 0 && !viewingShelf) eye.x += .18f * transition / .35f;
            worldCamera.fieldOfView = fov - 2 * focusBlend;
            worldCamera.transform.position = eye; worldCamera.transform.LookAt(look);
            foreach (var label in worldLabels)
            {
                if (label.rect == null || label.spot == null || !label.rect.gameObject.activeInHierarchy) continue;
                // Label beneath the model: the original mesh remains available for pointing/clicking.
                var screen = worldCamera.WorldToScreenPoint(label.spot.transform.position);
                float x = (screen.x - left) / scale;
                float y = 900 - (screen.y - bottom) / scale + 12;
                label.rect.anchoredPosition = new Vector2(Mathf.Clamp(x - label.width * .5f, 12, 1428 - label.width), -Mathf.Clamp(y, 100, 684));
            }
        }
        void WorldButton(WorldHotspot spot, string text, Color color, float width, bool enabled = true)
        {
            var button = ui.Button(text, 0, 0, width, 36, () => Activate(spot), color, enabled, 18);
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
            if ((mode != Mode.Menu && mode != Mode.Explore && mode != Mode.Puzzle) || transition > 0)
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
            if (spot != null && hoverText != null) hoverText.text = spot.Data.label;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (target != null) Collect(target);
                else if (spot != null) Activate(spot);
            }
        }
        string ChallengeInvitation() => map.id == "faroeste" ? "Visite o baú na Mina dos Brilhos." :
            map.id == "espaco" ? "Visite o rádio na Garagem das Naves." : "Visite a maquete no Quartel das Boas Ideias.";
        void Activate(WorldHotspot spot)
        {
            if (spot == null) return;
            var data = spot.Data;
            if (mode == Mode.Menu && data.action == "world")
            {
                EnterMap(catalog.maps.First(m => m.id == data.destination)); return;
            }
            if (mode == Mode.Explore)
            {
                if (data.action == "travel")
                {
                    int next = Array.FindIndex(map.rooms, r => r.id == data.destination);
                    if (next >= 0) { Tap(); roomIndex = next; selectedTarget = null; LoadRoom(); }
                    return;
                }
                if (data.action == "puzzle") { OpenPuzzle(); return; }
            }
            if (data.action == "inspect" && (mode == Mode.Menu || mode == Mode.Explore))
            {
                spot.React(); FocusOn(spot.transform.position, 2); Tap(); Tell(data.label); return;
            }
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
