using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace OlhosDeCacador
{
    public sealed partial class HunterGame : MonoBehaviour
    {
        enum Mode { Loading, Title, Menu, Junction, Explore, Pause, Options, Puzzle, Dialog, Victory, Error }
        const string SaveKey = "OlhosDeCacador.v1";
        Catalog catalog;
        Progress progress;
        HunterUI ui;
        PointerEventData pointerData;
        readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        Camera worldCamera;
        RoomView room;
        ToyShelf shelf;
        bool viewingShelf, autoNarration;
        RectTransform titlePlay;
        readonly List<RectTransform> titleStars = new List<RectTransform>();
        float titleEnteredAt;
        float effectsVolume = .5f, narrationVolume = 1f;
        Action optionsBack, dialogBack;
        [SerializeField] NarrationBank narration;
        AudioSource voice;
        Button listenButton;
        MapData map;
        int roomIndex;
        Mode mode;
        bool muted, reduceMotion;
        float transition;
        HuntTarget hovered;
        Text message, hoverText, puzzleMessage;
        AudioSource sound;
        AudioClip click, found, wrong;
        AudioClip[] notes;
        string selectedTarget, saveWarning = "";
        readonly Dictionary<string, int> hints = new Dictionary<string, int>();
        int[] digits = new int[3], assignments = { -1, -1, -1 };
        int selectedSupply = -1;
        MemorySequence memory;
        Coroutine signal;
        bool playingSignal;
        readonly string[] symbols = { "Lua", "Sol", "Estrela", "Planeta" };
        readonly string[] supplies = { "Antena", "Balde", "Martelo" };
        readonly string[] destinations = { "Praça", "Ponte", "Quartel" };

        void Start()
        {
            if (Instance != this) return;
            ui = new HunterUI(transform);
            // The package assigns its default UI actions when this module is enabled.
            var events = new GameObject("Eventos", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform);
            pointerData = new PointerEventData(events.GetComponent<EventSystem>());
            try
            {
                if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                    throw new InvalidOperationException("URP não está ativo. Execute Olhos de Cacador > 1. Preparar projeto no editor.");
                Application.targetFrameRate = 60;
                muted = PlayerPrefs.GetInt(SaveKey + ".mute", 0) == 1;
                reduceMotion = PlayerPrefs.GetInt(SaveKey + ".motion", 0) == 1;
                autoNarration = PlayerPrefs.GetInt(SaveKey + ".autoVoice", 0) == 1;
                effectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SaveKey + ".effectsVolume", .5f));
                narrationVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SaveKey + ".voiceVolume", 1f));
                if (narration == null) narration = Resources.Load<NarrationBank>("Hunter/Narration/NarrationBank");
                var data = Resources.Load<TextAsset>("Hunter/catalog");
                if (data == null) throw new InvalidOperationException("Catálogo não encontrado.");
                catalog = JsonUtility.FromJson<Catalog>(data.text);
                if (catalog?.maps == null || catalog.maps.Length != 3) throw new InvalidOperationException("Catálogo incompleto.");
                progress = LoadProgress();
                var background = new GameObject("Fundo", typeof(Camera)).GetComponent<Camera>();
                background.transform.SetParent(transform); background.depth = -10; background.cullingMask = 0;
                background.clearFlags = CameraClearFlags.SolidColor; background.backgroundColor = HunterUI.Paper;
                ConfigureCamera(background, false);
                worldCamera = new GameObject("Câmera do diorama", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                worldCamera.transform.SetParent(transform); worldCamera.tag = "MainCamera";
                worldCamera.clearFlags = CameraClearFlags.SolidColor; worldCamera.backgroundColor = HunterUI.Paper;
                worldCamera.fieldOfView = 43; worldCamera.nearClipPlane = .1f; worldCamera.farClipPlane = 60;
                ConfigureCamera(worldCamera, true);
                var light = new GameObject("Luz do quarto", typeof(Light)).GetComponent<Light>();
                light.transform.SetParent(transform); light.type = LightType.Directional; light.intensity = .95f;
                light.transform.rotation = Quaternion.Euler(45, -35, 0); light.shadows = LightShadows.Soft;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .44f, .52f);
                light.color = RoomView.Hex("#FFF0D9"); light.intensity = 1.2f; light.shadowStrength = .65f;
                // Shadows and MSAA are configured in HunterURP.asset, as required by URP.
                QualitySettings.vSyncCount = 0;
                sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.volume = effectsVolume;
                voice = gameObject.AddComponent<AudioSource>(); voice.playOnAwake = false;
                voice.loop = false; voice.spatialBlend = 0; ApplyAudioSettings();
                click = Clip("click1"); found = Clip("click3"); wrong = Clip("rollover1");
                notes = new[] { Clip("switch1"), Clip("switch4"), Clip("switch7"), Clip("switch10") };
                StartAtScene();
            }
            catch (Exception e) { ShowError(e); }
        }
        static void ConfigureCamera(Camera camera, bool shadows)
        {
            // Two independent Base cameras: full-window clear, then the diorama viewport.
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Base;
            data.SetRenderer(0);
            data.renderShadows = shadows;
            data.renderPostProcessing = false;
            data.requiresDepthTexture = false; data.requiresColorTexture = false;
            data.antialiasing = AntialiasingMode.None; // MSAA comes from the pipeline asset.
            camera.allowHDR = false; camera.allowMSAA = true;
        }
        AudioClip Clip(string name) => Resources.Load<AudioClip>("Hunter/Audio/" + name);
        void Play(AudioClip clip) { if (!muted && clip != null && sound != null) sound.PlayOneShot(clip); }
        void Tap() => Play(click);
        Progress LoadProgress()
        {
            var raw = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(raw)) return new Progress(catalog);
            try
            {
                var parsed = JsonUtility.FromJson<SaveData>(raw);
                if (parsed == null) throw new ArgumentException("Save vazio.");
                return new Progress(catalog, parsed);
            }
            catch
            {
                var backup = PlayerPrefs.GetString(SaveKey + ".backup", "");
                if (!string.IsNullOrEmpty(backup))
                {
                    try
                    {
                        var parsed = JsonUtility.FromJson<SaveData>(backup);
                        if (parsed == null) throw new ArgumentException("Backup vazio.");
                        var recovered = new Progress(catalog, parsed);
                        saveWarning = "Seu último progresso válido foi recuperado.";
                        return recovered;
                    }
                    catch { /* Both copies invalid: preserve playability. */ }
                }
                saveWarning = "Não foi possível ler o progresso anterior. Uma nova brincadeira está disponível.";
                return new Progress(catalog);
            }
        }
        void Save()
        {
            try
            {
                // Serialize a validated in-memory state into both slots, never back up a corrupt file.
                string encoded = JsonUtility.ToJson(progress.Data);
                PlayerPrefs.SetString(SaveKey + ".backup", encoded);
                PlayerPrefs.SetString(SaveKey, encoded);
                PlayerPrefs.SetInt(SaveKey + ".mute", muted ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".motion", reduceMotion ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".autoVoice", autoNarration ? 1 : 0);
                PlayerPrefs.SetFloat(SaveKey + ".effectsVolume", effectsVolume);
                PlayerPrefs.SetFloat(SaveKey + ".voiceVolume", narrationVolume);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Progresso disponível nesta sessão; falha ao salvar: " + e.Message);
                saveWarning = "Não foi possível salvar neste computador. Você pode continuar nesta sessão.";
            }
        }
        void Update()
        {
            if (worldCamera == null) return;
            UpdateStageCamera();
            transition = Mathf.Max(0, transition - Time.unscaledDeltaTime);
            var keyboard = Keyboard.current;
            if (mode == Mode.Title)
            {
                float elapsed = Time.unscaledTime - titleEnteredAt;
                shelf?.Pose(elapsed, !reduceMotion);
                if (titlePlay != null) titlePlay.localScale = Vector3.one * (reduceMotion ? 1 : 1 + Mathf.Sin(elapsed * 2) * .015f);
                for (int i = 0; i < titleStars.Count; i++)
                    if (titleStars[i] != null) titleStars[i].localRotation = Quaternion.Euler(0, 0, reduceMotion ? 0 : Mathf.Sin(elapsed * 1.2f + i) * 8);
                var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                if (ui.LastActionFrame != Time.frameCount && keyboard != null &&
                    (selected == null || (titlePlay != null && selected == titlePlay.gameObject)) &&
                    (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) BeginAdventure();
                return;
            }
            if (listenButton != null) listenButton.GetComponentInChildren<Text>().text = voice != null && voice.isPlaying ? "Parar leitura" : "Ouvir charada";
            if (ui.LastActionFrame == Time.frameCount) { ClearWorldHover(); return; }
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (mode == Mode.Explore || mode == Mode.Junction) ShowPause();
                else if (mode == Mode.Options) optionsBack?.Invoke();
                else if (mode == Mode.Dialog) (dialogBack ?? ReturnToWorld).Invoke();
                else if (mode == Mode.Pause || mode == Mode.Puzzle) ReturnToWorld();
                else if (mode == Mode.Menu) ShowTitle();
                return;
            }
            UpdateWorldInput();
        }
        bool PointerOverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            pointerData.position = position;
            uiHits.Clear();
            // Query this frame's position directly, independent of EventSystem.Update order.
            EventSystem.current.RaycastAll(pointerData, uiHits);
            return uiHits.Count != 0;
        }
        void SetHover(HuntTarget target)
        {
            if (hovered != target)
            {
                if (hovered != null) hovered.Hover(false);
                hovered = target;
                // No automatic enlargement/reveal on hover: the clue, not the cursor, identifies a target.
            }
            if (hoverText != null)
                hoverText.text = Time.unscaledTime < messageUntil && !string.IsNullOrEmpty(lastMessage) ? lastMessage :
                    mode == Mode.Menu ? "Clique numa maquete para entrar. Cada brinquedo abre um mundo." :
                    mode == Mode.Junction ? "Explore as portas e passagens da maquete. A roda do mouse aproxima." :
                    "Observe os detalhes. A roda do mouse aproxima; botão direito afasta.";
        }
        void Collect(HuntTarget target)
        {
            var data = target.Data;
            if (!progress.Collect(data.id)) return;
            FocusOn(target.transform.position, 1.1f);
            SetHover(null); room.Targets.Remove(data.id); target.Celebrate();
            Save(); Play(found); DrawHUD();
            Tell(progress.Ready(map) ? "Todos encontrados! " + ChallengeInvitation() : "Achei! " + data.label + (string.IsNullOrEmpty(data.clue) ? "" : " • Pista anotada no caderno."));
        }
        float messageUntil;
        void Tell(string value)
        {
            lastMessage = value;
            if (message != null) message.text = value;
            messageUntil = Time.unscaledTime + 8;
        }
        void StopSignal()
        {
            if (signal != null) StopCoroutine(signal);
            signal = null; playingSignal = false;
        }
        bool PrepareBedroom(Mode screen)
        {
            StopSignal(); StopNarration(); CloseChallenge(); ClearWorldHover(); mode = screen; focusUntil = 0; focusBlend = 0;
            titlePlay = null; titleStars.Clear(); worldLabels.Clear(); room?.Dispose(); shelf?.Dispose(); shelf = null;
            map = null; roomIndex = -1; viewingShelf = false; currentStage = catalog.hub; ResetMagnifier();
            try { room = new RoomView(new MapData { color = "#AD537E", floor = "#E9C696", wall = "#A8CFD8" }, currentStage, progress, reduceMotion); }
            catch (Exception e) { ShowError(e); return false; }
            worldCamera.backgroundColor = RoomView.Hex("#AACBD7"); return true;
        }
        void ShowTitle()
        {
            if (RouteTo("inicio", ShowTitle, room != null)) return;
            if (!PrepareBedroom(Mode.Title)) return;
            titleEnteredAt = Time.unscaledTime; ui.Clear();
            ui.Panel(70, 88, 610, 370, new Color(.14f, .16f, .32f, .91f));
            ui.Panel(70, 88, 610, 10, RoomView.Hex("#FFD786"));
            ui.Label("OLHOS DE", 106, 123, 538, 70, 52, HunterUI.Paper, true, TextAnchor.MiddleCenter);
            ui.Label("CAÇADOR", 94, 186, 562, 110, 88, RoomView.Hex("#FFCF79"), true, TextAnchor.MiddleCenter);
            ui.Label("Um quarto cheio de mundos para descobrir.", 108, 312, 532, 68, 24, Color.white, false, TextAnchor.MiddleCenter);
            var play = ui.Button("Iniciar", 158, 488, 430, 82, BeginAdventure, RoomView.Hex("#B44264"), true, 37);
            titlePlay = play.GetComponent<RectTransform>();
            ui.Button("Opções", 158, 592, 430, 66, () => ShowOptions(ShowTitle), RoomView.Hex("#247B91"), true, 29);
            ui.Button("Como jogar", 158, 676, 430, 66, () => Help(ShowTitle), RoomView.Hex("#6F4D9C"), true, 29);
            TitleStar(622, 62, 48); TitleStar(98, 405, 35);
            ui.Label("Enter também inicia a aventura", 126, 770, 502, 32, 18, HunterUI.Ink, false, TextAnchor.MiddleCenter);
            if (!string.IsNullOrEmpty(saveWarning)) ui.Label(saveWarning, 710, 808, 670, 62, 20, HunterUI.Ink);
        }

        void TitleStar(float x, float y, float size)
        {
            titleStars.Add(ui.Art("minimap_icon_star_yellow", x, y, size, size).rectTransform);
        }
        void BeginAdventure()
        {
            // A submit event and the Enter shortcut can arrive in the same frame.
            if (mode != Mode.Title) return;
            Tap(); ShowMenu();
        }
        void ShowMenu()
        {
            if (RouteTo("quarto", ShowMenu, room != null)) return;
            if (!PrepareBedroom(Mode.Menu)) return;
            ui.Clear();
            ui.Panel(0, 0, 1440, 56, HunterUI.Plum);
            ui.Label("O QUARTO DAS BRINCADEIRAS", 28, 8, 1000, 42, 29, HunterUI.Paper, true);
            ui.Button("Tela inicial", 1210, 8, 204, 40, ShowTitle, HunterUI.Teal, true, 19);
            ui.Panel(0, 800, 1440, 100, RoomView.Hex("#F7DFAD"));
            hoverText = ui.Label("Qual brinquedo vai abrir sua próxima aventura?", 40, 808, 1360, 30, 21, HunterUI.Ink, false, TextAnchor.MiddleCenter);
            ui.Button("Opções", 60, 850, 280, 36, () => ShowOptions(ShowMenu), HunterUI.Teal, true, 20);
            ui.Button("Créditos", 370, 850, 280, 36, () => Credits(ShowMenu), RoomView.Hex("#AF4662"), true, 20);
            ui.Button("Novo jogo", 680, 850, 340, 36, () => Confirm("Uma nova aventura?", "Isso limpa as descobertas e os caminhos abertos dos três mundos.", () => { progress.ResetAll(); hints.Clear(); Save(); ShowMenu(); }, ShowMenu), RoomView.Hex("#745090"), true, 20);
            ui.Button("Sair", 1050, 850, 330, 36, () => ConfirmExit(ShowMenu), RoomView.Hex("#93603D"), true, 20);
        }

        void EnterMap(MapData selected)
        {
            Tap(); map = selected; roomIndex = -1; selectedTarget = null;
            ShowJunction();
        }

        bool LoadRoom()
        {
            if (RouteTo(map.rooms[roomIndex].id, () => LoadRoom(), room != null)) return false;
            try
            {
                StopSignal(); StopNarration(); ClearWorldHover(); CloseChallenge(); room?.Dispose();
                currentStage = map.rooms[roomIndex]; ResetMagnifier(); focusUntil = 0; messageUntil = 0; lastMessage = "";
                room = new RoomView(map, currentStage, progress, reduceMotion);
                worldCamera.backgroundColor = RoomView.Hex(map.wall);
                transition = reduceMotion ? 0 : .35f; mode = Mode.Explore; DrawHUD();
                if (autoNarration) PlayNarration(); return true;
            }
            catch (Exception e) { ShowError(e); return false; }
        }
        void ReturnToWorld()
        {
            StopSignal(); Tap();
            if (map != null && roomIndex < 0)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == map.junction.id && room != null)
                { room.SetReducedMotion(reduceMotion); DrawJunction(); }
                else ShowJunction();
                return;
            }
            if (map == null) { ShowMenu(); return; }
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != map.rooms[roomIndex].id || room == null) { LoadRoom(); return; }
            CloseChallenge(); currentStage = map.rooms[roomIndex]; focusUntil = 0; room.SetReducedMotion(reduceMotion);
            if (room?.Root != null) room.Root.SetActive(true);
            mode = Mode.Explore; DrawHUD(); if (autoNarration) PlayNarration();
        }
        void DrawHUD()
        {
            mode = Mode.Explore; ui.Clear(); worldLabels.Clear(); listenButton = null;
            Color accent = RoomView.Hex(currentStage.color);
            var currentRoom = map.rooms[roomIndex];
            var targets = currentRoom.objects.Where(p => !string.IsNullOrEmpty(p.id)).ToArray();
            int count = targets.Count(p => progress.Has(p.id));
            ui.Panel(0, 0, 1440, 56, accent);
            ui.Label(currentRoom.name, 24, 8, 660, 40, 27, Color.white, true);
            ui.Label(count + " / " + targets.Length + " neste cantinho", 690, 16, 245, 30, 20, HunterUI.Paper);
            ui.Label("Explore as passagens", 952, 10, 150, 36, 16, Color.white);
            ui.Button("Caderno", 1110, 8, 150, 40, Notebook, HunterUI.Plum, true, 18);
            ui.Button("Pausa", 1268, 8, 148, 40, ShowPause, RoomView.Hex("#AE496C"), true, 18);
            ui.Panel(0, 730, 1440, 170, accent); ui.Sheet(8, 736, 958, 156);
            string[] lines = currentRoom.riddle.Split('\n');
            ui.Label(string.Join("\n", lines.Take(3)), 24, 748, 456, 90, 19, HunterUI.Ink);
            ui.Label(string.Join("\n", lines.Skip(3)), 496, 748, 450, 90, 19, HunterUI.Ink);
            Color[] colors = { HunterUI.Teal, HunterUI.Plum, RoomView.Hex("#AD4764"), RoomView.Hex("#3C679B"), RoomView.Hex("#8B5B30"), RoomView.Hex("#3E7550") };
            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i]; bool collected = progress.Has(target.id);
                ui.Button(collected ? "Achei " + (i + 1) : "Dica " + (i + 1), 24 + i * 155, 846, 146, 36,
                    () => { selectedTarget = target.id; Hint(); }, colors[i % colors.Length], !collected, 18);
            }
            ui.Sheet(978, 736, 438, 156);
            ui.Label("VOZ DA AVENTURA", 998, 752, 398, 30, 23, accent, true, TextAnchor.MiddleCenter);
            ui.Button("Ler juntos", 998, 814, 190, 52, ShowReading, HunterUI.Plum, true, 20);
            listenButton = ui.Button(CurrentReading() != null ? "Ouvir charada" : "Sem gravação", 1200, 814, 194, 52, ToggleNarration, HunterUI.Teal, CurrentReading() != null, 18);
            if (CurrentReading() == null) listenButton = null;
            ui.Panel(280, 684, 880, 34, new Color(.08f, .10f, .21f, .82f));
            hoverText = ui.Label(count == targets.Length ? "Charada completa! Experimente o mecanismo desta maquete." : "Seis tesouros, muitos brinquedos. Observe antes de clicar!", 292, 686, 856, 30, 18, Color.white, false, TextAnchor.MiddleCenter);
            message = null;
            if (!string.IsNullOrEmpty(saveWarning)) Tell(saveWarning);
        }

        void Hint()
        {
            if (progress.RoomReady(map.rooms[roomIndex])) { Tell("Os seis objetos já foram encontrados. Experimente os mecanismos e as passagens!"); return; }
            Tap();
            var target = map.Targets.FirstOrDefault(p => p.id == selectedTarget && !progress.Has(p.id))
                ?? map.rooms[roomIndex].objects.FirstOrDefault(p => !string.IsNullOrEmpty(p.id) && !progress.Has(p.id))
                ?? map.Targets.First(p => !progress.Has(p.id));
            selectedTarget = target.id;
            int stage = hints.TryGetValue(target.id, out int value) ? value : 0; hints[target.id] = stage + 1;
            int targetRoom = Array.FindIndex(map.rooms, r => r.objects.Any(p => p.id == target.id));
            if (stage == 0 || targetRoom != roomIndex) { Tell("Pista: " + target.label + " está em " + map.rooms[targetRoom].name + "."); return; }
            Tell(target.hint);
            if (stage >= 2 && room.Targets.TryGetValue(target.id, out var instance)) instance.Hint();
        }
        void ShowPause()
        {
            StopNarration(); Tap(); mode = Mode.Pause; SetHover(null); ui.Modal("Uma pausa na brincadeira", "Suas descobertas estão guardadas. Volte quando quiser.");
            ui.Button("Continuar", 242, 354, 956, 62, ReturnToWorld, RoomView.Hex(map.color));
            ui.Button("Como jogar", 242, 434, 956, 60, () => Help(ShowPause));
            ui.Button("Recomeçar este mapa", 242, 514, 956, 60, () => Confirm("Recomeçar este mundo?", "Os outros mundos continuam guardados.", () => { progress.ResetMap(map); foreach (var p in map.Targets) hints.Remove(p.id); roomIndex = -1; Save(); ShowJunction(); }, ShowPause));
            ui.Button("Opções de áudio e movimento", 242, 594, 956, 60, () => ShowOptions(() => { ReturnToWorld(); ShowPause(); }));
            ui.Button("Sair do jogo", 242, 674, 956, 60, () => ConfirmExit(ShowPause));
        }
        void ConfirmExit(Action back)
        {
            Confirm("Fechar a caixa de brinquedos?", "Quer sair do jogo e voltar para o Windows?", ExitGame, back, "Sim, sair");
        }
        void ExitGame()
        {
            StopSignal(); StopNarration(); if (progress != null) Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void ApplyAudioSettings()
        {
            if (sound != null) { sound.volume = effectsVolume; sound.mute = muted; }
            if (voice != null) { voice.volume = narrationVolume; voice.mute = muted; }
        }
        void ShowOptions(Action back)
        {
            StopSignal(); StopNarration(); optionsBack = back; mode = Mode.Options;
            ui.Backdrop("Do seu jeitinho", "Ajuste a brincadeira. Suas descobertas continuam guardadas.");
            ui.Sheet(150, 202, 1140, 536);
            ui.Label("SONS DOS BRINQUEDOS", 196, 238, 650, 34, 24, HunterUI.Teal, true);
            VolumeRow(292, effectsVolume, value => { effectsVolume = value; ApplyAudioSettings(); Save(); Play(click); ShowOptions(back); });
            ui.Label("VOZ DO NARRADOR", 196, 382, 650, 34, 24, HunterUI.Plum, true);
            VolumeRow(436, narrationVolume, value => { narrationVolume = value; ApplyAudioSettings(); Save(); ShowOptions(back); });
            ui.Button(muted ? "Áudio desligado" : "Áudio ligado", 916, 264, 324, 62,
                () => { muted = !muted; ApplyAudioSettings(); Save(); ShowOptions(back); }, HunterUI.Teal);
            ui.Button(autoNarration ? "Leitura automática: sim" : "Leitura automática: não", 916, 418, 324, 78,
                () => { autoNarration = !autoNarration; Save(); ShowOptions(back); }, HunterUI.Plum, true, 20);
            ui.Label("A leitura automática usa a voz gravada ao entrar em cada cantinho.", 196, 541, 1044, 50, 21, HunterUI.Muted);
            ui.Button(reduceMotion ? "Movimento reduzido" : "Movimento suave", 196, 622, 490, 64,
                () => { reduceMotion = !reduceMotion; Save(); ShowOptions(back); }, HunterUI.Teal);
            ui.Label("Reduz o movimento da câmera, dos brinquedos e das dicas.", 722, 622, 510, 72, 22, HunterUI.Muted);
            ui.Button("Pronto! Voltar à brincadeira", 420, 782, 600, 68, back, HunterUI.Plum, true, 25);
        }
        void VolumeRow(float y, float value, Action<float> changed)
        {
            ui.Button("−", 196, y, 66, 56, () => changed(Mathf.Clamp01(value - .1f)), HunterUI.Ink, value > 0, 30);
            for (int i = 0; i < 10; i++) ui.Panel(282 + i * 42, y + 10, 32, 36, value >= (i + 1) * .1f - .001f ? HunterUI.Teal : new Color(.76f, .72f, .62f));
            ui.Button("+", 720, y, 66, 56, () => changed(Mathf.Clamp01(value + .1f)), HunterUI.Ink, value < 1, 30);
            ui.Label(Mathf.RoundToInt(value * 100) + "%", 800, y + 12, 100, 36, 24, HunterUI.Ink, true);
        }
        AudioClip CurrentReading() => map != null && roomIndex >= 0 && narration != null ? narration.ForRoom(map.rooms[roomIndex].id) : null;
        void StopNarration()
        {
            if (voice != null) voice.Stop();
        }
        void PlayNarration()
        {
            var clip = CurrentReading();
            if (voice == null || clip == null || muted || narrationVolume <= 0) return;
            voice.Stop(); voice.clip = clip; voice.Play();
        }
        void ToggleNarration()
        {
            if (voice != null && voice.isPlaying) { voice.Stop(); return; }
            if (muted || narrationVolume <= 0) { Tell("O áudio está desligado. Você pode ajustá-lo em Pausa > Opções."); return; }
            PlayNarration();
        }
        void ShowReading()
        {
            StopNarration(); dialogBack = ReturnToWorld; mode = Mode.Dialog;
            ui.Modal("Vamos ler juntos?", map.rooms[roomIndex].name + " • Uma pessoa lê a charada; a turma procura os tesouros.");
            ui.Label(map.rooms[roomIndex].riddle, 244, 350, 948, 244, 33, HunterUI.Ink, false, TextAnchor.MiddleLeft);
            ui.Button("Voltar e procurar", 242, 674, 956, 62, ReturnToWorld, RoomView.Hex(map.color));
        }
        void Help(Action back)
        {
            StopNarration(); dialogBack = back; mode = Mode.Dialog; ui.Modal("Como jogar", "Entre numa maquete do quarto e explore suas portas, pontes e trilhos.");
            ui.Label("• Cada maquete esconde SEIS objetos. Leia ou ouça a charada.\n• Clique nas portas, trilhos e pontes para caminhar pelo mundo.\n• Alguns brinquedos só funcionam com uma peça encontrada.\n• Complete a charada e clique no mecanismo para usar a peça.\n• Abra atalhos e volte para observar o que mudou.\n• Roda do mouse: aproximar. Botão direito: visão geral.\n• Dica 1 a 6 ajuda em etapas. Encontre os 18 objetos e o final.", 242, 334, 946, 294, 23);
            ui.Button("Entendi!", 242, 674, 956, 62, back);
        }
        void Notebook()
        {
            StopNarration(); dialogBack = ReturnToWorld; Tap(); mode = Mode.Dialog; ui.Modal("Caderno de descobertas", "As pistas encontradas ficam aqui. Cada mundo guarda sua própria brincadeira.");
            var clues = map.Targets.Where(p => progress.Has(p.id) && !string.IsNullOrEmpty(p.clue)).Select(p => "• " + p.clue).ToArray();
            ui.Label(clues.Length == 0 ? "Ainda não há anotações. Explore os objetos das três telas!" : string.Join("\n\n", clues), 242, 348, 948, 276, 27);
            ui.Button("Voltar à caça", 242, 674, 956, 62, ReturnToWorld);
        }
        void Credits(Action back)
        {
            StopNarration(); dialogBack = back; mode = Mode.Dialog; ui.Modal("Feito para brincar", "Assets prontos de Kenney e Poly Haven; licenças nos arquivos do projeto.");
            ui.Label("Kenney: Survival, Nature, Space, City, Blocky Characters\nFurniture Kit • Toy Car Kit • Food Kit • UI e áudio\nPato de borracha: Plat251 • CC0 • Powered by Poly Haven\nFonte Lilita One: Juan Montoreano • SIL OFL 1.1\n\nInspiração de gênero: livros I Spy e I Spy: Fantasy.\nNenhum conteúdo desses livros ou jogos foi copiado.\n\nConceito solicitado por Gabriel. Código com assistência de IA.\nA voz da aventura é uma gravação humana opcional.", 242, 338, 950, 320, 22);
            ui.Button("Voltar", 242, 674, 956, 62, back);
        }
        void Confirm(string title, string body, Action yes, Action no, string confirmLabel = "Sim, recomeçar")
        {
            StopNarration(); dialogBack = no; mode = Mode.Dialog; ui.Modal(title, body);
            ui.Button("Cancelar", 242, 556, 468, 66, no);
            ui.Button(confirmLabel, 730, 556, 468, 66, yes, new Color(.62f, .33f, .25f));
        }
        void OpenPuzzle()
        {
            if (!progress.Ready(map)) { Tell("Faltam " + (map.Targets.Count() - progress.Count(map)) + " tesouros nos três cantinhos. Depois volte aqui!"); return; }
            if (map.gates.Any(g => !progress.Opened(g.id))) { Tell("Os objetos já estão guardados. Faça os dois mecanismos funcionarem antes do desafio final."); return; }
            if (progress.Solved(map)) { Tell("O selo deste mundo já é seu! Você pode continuar explorando."); return; }
            if (RouteTo(map.challenge.id, OpenPuzzle)) return;
            StopNarration(); StopSignal(); ClearWorldHover(); Tap();
            digits = new int[3]; assignments = new[] { -1, -1, -1 }; selectedSupply = -1;
            memory = new MemorySequence(map.solution);
            try
            {
                currentStage = map.challenge; ResetMagnifier(); focusUntil = 0;
                if (room != null) room.Root.SetActive(false);
                challengeView = new RoomView(map, currentStage, progress, reduceMotion);
                DrawPuzzle();
            }
            catch (Exception e) { ShowError(e); }
        }
        void DrawPuzzle()
        {
            mode = Mode.Puzzle; ui.Clear(); worldLabels.Clear();
            Color accent = RoomView.Hex(map.color);
            ui.Panel(0, 0, 1440, 56, accent);
            ui.Label(map.puzzleTitle, 28, 10, 1100, 36, 28, Color.white, true);
            ui.Label("Saída pelo cenário", 1190, 12, 226, 36, 18, Color.white);
            ui.Panel(0, 760, 1440, 140, accent);
            ui.Sheet(12, 768, 1416, 120);
            string intro = map.puzzle == "lock" ? "Clique nas três garrafinhas para mudar os números. Depois clique no baú." :
                map.puzzle == "memory" ? "Veja quais brinquedos recebem o sinal. Clique neles na mesma ordem." :
                "Clique numa ferramenta e depois na miniatura do lugar que precisa dela.";
            ui.Label(intro, 32, 780, 1040, 38, 23, HunterUI.Ink, true);
            puzzleMessage = ui.Label(map.puzzle == "lock" ? "Pistas: " + string.Join("  •  ", map.clues) :
                map.puzzle == "matching" ? "Praça: flores com sede. Ponte: conserto. Quartel: mensagem." : "Use Assistir / repetir para ouvir e ver a mensagem.", 32, 830, 1040, 50, 19, HunterUI.Ink);
            if (map.puzzle == "memory") ui.Button("Assistir / repetir", 1100, 790, 296, 68, () => { StopSignal(); signal = StartCoroutine(ShowSignal()); }, accent, true, 23);
            else ui.Label("Clique na saída para voltar", 1100, 790, 296, 68, 21, HunterUI.Ink);
            foreach (var spot in challengeView.Hotspots.Where(h => h.Data.action != "exit"))
            {
                string label = spot.Data.label;
                if (spot.Data.action == "digit") label = (spot.Data.value + 1) + "º:  " + digits[spot.Data.value] + "  +";
                if (spot.Data.action == "supply") label = (selectedSupply == spot.Data.value ? "> " : "") + label;
                if (spot.Data.action == "destination" && assignments[spot.Data.value] >= 0) label += "  OK";
                if (spot.Data.action == "inspect" || spot.Data.action == "celebrate") continue;
                bool enabled = spot.Data.action != "supply" || !assignments.Contains(spot.Data.value);
                WorldButton(spot, label, spot.Data.action == "supply" && selectedSupply == spot.Data.value ? HunterUI.Plum : accent, 210, enabled);
            }
        }
        IEnumerator ShowSignal()
        {
            playingSignal = true; memory.Reset();
            for (int i = 0; i < map.solution.Length; i++)
            {
                int symbol = map.solution[i];
                puzzleMessage.text = "Sinal " + (i + 1) + " de 4: " + symbols[symbol];
                var spot = challengeView.Hotspots.First(h => h.Data.action == "signal" && h.Data.value == symbol);
                spot.Mark(.85f); if (!reduceMotion) spot.React();
                Play(notes[symbol]); yield return new WaitForSecondsRealtime(.9f);
                puzzleMessage.text = "Observe..."; yield return new WaitForSecondsRealtime(.25f);
            }
            playingSignal = false; signal = null; puzzleMessage.text = "Sua vez! Repita os quatro sinais.";
        }
        void PressSignal(int index)
        {
            if (playingSignal) return;
            Play(notes[index]);
            if (!memory.Press(index)) { puzzleMessage.text = "Tente de novo. Use Assistir / repetir para rever o sinal."; Play(wrong); return; }
            puzzleMessage.text = "Muito bem! " + memory.Position + " de 4 sinais.";
            if (memory.Complete) Win();
        }
        void Match(int destination)
        {
            if (selectedSupply < 0) { puzzleMessage.text = "Primeiro escolha uma ferramenta."; return; }
            if (assignments[destination] >= 0) return;
            if (!PuzzleRules.Match(selectedSupply, destination, map.solution))
            { Play(wrong); puzzleMessage.text = "Pense no que esse lugar precisa e tente outra combinação."; return; }
            var item = challengeView.Hotspots.First(h => h.Data.action == "supply" && h.Data.value == selectedSupply);
            var place = challengeView.Hotspots.First(h => h.Data.action == "destination" && h.Data.value == destination);
            item.PlaceAt(place.transform.localPosition + new Vector3(.7f, 0, -.6f)); place.React();
            assignments[destination] = selectedSupply; selectedSupply = -1; Play(found);
            if (assignments.All(a => a >= 0)) Win(); else DrawPuzzle();
        }
        void Win()
        {
            StopSignal(); StopNarration(); ClearWorldHover(); if (!progress.Complete(map)) return;
            Save(); Play(found); mode = Mode.Victory;
            foreach (var spot in challengeView.Hotspots) spot.React();
            ui.Clear(); worldLabels.Clear();
            ui.Panel(0, 0, 1440, 96, HunterUI.Plum);
            ui.Label(progress.Finished ? "VOCÊ TEM OLHOS DE CAÇADOR!" : "MAIS UMA BRINCADEIRA COMPLETA!", 100, 24, 1240, 56, 38, HunterUI.Paper, true, TextAnchor.MiddleCenter);
            ui.Art("minimap_icon_star_yellow", 32, 20, 60, 60);
            ui.Art("minimap_icon_star_yellow", 1348, 20, 60, 60);
            ui.Panel(0, 724, 1440, 176, HunterUI.Plum);
            string badge = map.id == "faroeste" ? "XERIFE DA BRINCADEIRA" : map.id == "espaco" ? "EXPLORADOR LUNAR" : "HERÓI DO QUINTAL";
            ui.Label(badge + (progress.Finished ? " • OS TRÊS SELOS SÃO SEUS!" : " • SELO CONQUISTADO"), 80, 742, 1280, 42, 30, HunterUI.Paper, true, TextAnchor.MiddleCenter);
            ui.Label("O selo é seu! Continue explorando as passagens.", 280, 814, 420, 62, 21, HunterUI.Paper);
            ui.Button("Explorar o cenário", 740, 814, 420, 62, ReturnToWorld, HunterUI.Teal, true, 23);
        }

        void ShowError(Exception exception)
        {
            Debug.LogException(exception); StopSignal(); StopNarration(); mode = Mode.Error;
            ui.Modal("Uma peça não carregou", "Confira se todos os arquivos do projeto foram importados. Suas descobertas anteriores continuam salvas.");
            ui.Label(exception.Message, 242, 352, 950, 180, 24);
            ui.Button("Tentar novamente", 242, 674, 956, 62, RestartSession);
        }
        void OnApplicationFocus(bool focused) { if (!focused) StopNarration(); }
        void OnApplicationPause(bool paused) { if (paused) { StopNarration(); if (progress != null) Save(); } }
        void OnApplicationQuit() { if (progress != null) Save(); }
        void OnDestroy() { if (Instance == this) Instance = null; StopSignal(); StopNarration(); room?.Dispose(); shelf?.Dispose(); challengeView?.Dispose(); }
    }
}
