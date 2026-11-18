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
                InitializeRoomAudio();
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
        void Play(AudioClip clip) { if (!muted && clip != null && sound != null) sound.PlayOneShot(clip, voice != null && voice.isPlaying ? .35f : .65f); }
        void Tap() => Play(click);
        Progress LoadProgress()
        {
            // Cada execucao comeca com uma nova partida. Nao reutilizar objetos,
            // mecanismos ou desafios concluidos de execucoes anteriores.
            // As preferencias de audio/acessibilidade usam chaves separadas.
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(SaveKey + ".backup");
            PlayerPrefs.Save();
            return new Progress(catalog);
        }
        void Save()
        {
            try
            {
                // O progresso permanece somente em memoria durante a partida.
                // Salvar apenas preferencias persistentes entre execucoes.
                PlayerPrefs.SetInt(SaveKey + ".mute", muted ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".motion", reduceMotion ? 1 : 0);
                PlayerPrefs.SetInt(SaveKey + ".autoVoice", autoNarration ? 1 : 0);
                PlayerPrefs.SetFloat(SaveKey + ".effectsVolume", effectsVolume);
                PlayerPrefs.SetFloat(SaveKey + ".voiceVolume", narrationVolume);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Nao foi possivel salvar as preferencias: " + e.Message);
                saveWarning = "Nao foi possivel salvar as preferencias neste computador.";
            }
        }
        void Update()
        {
            if (worldCamera == null) return;
            UpdateStageCamera();
            UpdateRoomAudio();
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
                hoverText.text = Time.unscaledTime < messageUntil ? lastMessage : "";
            UpdateFeedbackVisibility();
        }
        void Collect(HuntTarget target)
        {
            var data = target.Data;
            if (!progress.Collect(data.id)) return;
            SetHover(null); room.Targets.Remove(data.id); target.Celebrate();
            Save(); PlayObjectSound(data, true); DrawHUD();
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
            StopSignal(); StopNarration(); CloseChallenge(); ClearWorldHover(); mode = screen; 
            titlePlay = null; titleStars.Clear(); worldLabels.Clear(); room?.Dispose(); shelf?.Dispose(); shelf = null;
            map = null; roomIndex = -1; viewingShelf = false; currentStage = catalog.hub; 
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
            ui.Label("Escolha uma brincadeira", 32, 22, 670, 46, 32, HunterUI.Paper, true);
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, HunterUI.Plum, true, 21);
            ui.Button("Início", 1188, 18, 108, 42, ShowTitle, HunterUI.Teal, true, 21);
            CreateFeedback();
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
                currentStage = map.rooms[roomIndex];  messageUntil = 0; lastMessage = "";
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
            CloseChallenge(); currentStage = map.rooms[roomIndex];  room.SetReducedMotion(reduceMotion);
            if (room?.Root != null) room.Root.SetActive(true);
            mode = Mode.Explore; DrawHUD(); if (autoNarration) PlayNarration();
        }
        void DrawHUD()
        {
            mode = Mode.Explore; ui.Clear(); worldLabels.Clear(); listenButton = null;
            var currentRoom = map.rooms[roomIndex];
            var targets = currentRoom.objects.Where(p => !string.IsNullOrEmpty(p.id)).ToArray();
            // The lower band belongs only to the riddle, as in a picture-book search.
            ui.Panel(0, 760, 1440, 140, HunterUI.Paper).raycastTarget = true;
            ui.Panel(0, 760, 1440, 3, RoomView.Hex(currentStage.color));
            string[] lines = currentRoom.riddle.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                bool collected = i < targets.Length && progress.Has(targets[i].id);
                string line = (collected ? "• " : "") + lines[i];
                ui.Label(line, i < 3 ? 38 : 742, 778 + (i % 3) * 36, 660, 35, 27,
                    collected ? HunterUI.Teal : HunterUI.Ink);
            }
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, HunterUI.Plum, true, 21);
            CreateFeedback(); message = null;
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
            StopNarration(); Tap(); mode = Mode.Pause; SetHover(null);
            ui.Modal("Uma pausa na brincadeira", map == null ? "O quarto guarda suas três aventuras." :
                map.title + " • " + progress.Count(map) + " de 18 descobertas");
            ui.Button("Continuar", 242, 342, 956, 56, ReturnToWorld, HunterUI.Teal, true, 27);
            bool searching = map != null && roomIndex >= 0 && currentStage == map.rooms[roomIndex];
            if (searching)
            {
                ui.Button("Ouvir charada", 242, 414, 468, 54, () => { ReturnToWorld(); if (muted || narrationVolume <= 0) Tell("A voz está desligada. Ajuste o volume em Menu > Opções."); else PlayNarration(); }, HunterUI.Plum, CurrentReading() != null);
                ui.Button("Ler juntos", 730, 414, 468, 54, ShowReading, HunterUI.Plum);
                ui.Button("Uma ajudinha", 242, 482, 468, 54, ShowHints, HunterUI.Teal);
                ui.Button("Caderno de pistas", 730, 482, 468, 54, Notebook, HunterUI.Teal);
            }
            else
            {
                ui.Button("Como jogar", 242, 414, 468, 54, () => Help(ShowPause));
                ui.Button("Créditos", 730, 414, 468, 54, () => Credits(ShowPause));
                ui.Label("As portas, pontes e trilhos levam aos próximos cantinhos.", 242, 490, 956, 48, 25);
            }
            ui.Button("Opções", 242, 550, 468, 54, () => ShowOptions(ShowPause), HunterUI.Plum);
            ui.Button("Como jogar", 730, 550, 468, 54, () => Help(ShowPause), HunterUI.Plum);
            ui.Button(map == null ? "Nova brincadeira" : "Recomeçar este mundo", 242, 618, 468, 54,
                () => Confirm("Recomeçar?", map == null ? "Isso limpa o progresso dos três mundos." : "Os outros mundos continuam guardados.",
                    () => { if (map == null) { progress.ResetAll(); hints.Clear(); Save(); ShowMenu(); }
                        else { progress.ResetMap(map); foreach (var target in map.Targets) hints.Remove(target.id); roomIndex = -1; Save(); ShowJunction(); } }, ShowPause));
            ui.Button("Sair do jogo", 730, 618, 468, 54, () => ConfirmExit(ShowPause));
        }
        void ShowHints()
        {
            mode = Mode.Dialog; dialogBack = ShowPause;
            ui.Modal("Uma ajudinha", "Escolha um verso. A primeira dica diz o nome; as seguintes ajudam a localizar o objeto.");
            var targets = map.rooms[roomIndex].objects.Where(p => !string.IsNullOrEmpty(p.id)).ToArray();
            string[] lines = map.rooms[roomIndex].riddle.Split('\n');
            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i]; bool collected = progress.Has(target.id);
                ui.Button((collected ? "• " : "") + lines[i], 242, 336 + i * 53, 956, 44,
                    () => { selectedTarget = target.id; ReturnToWorld(); Hint(); }, HunterUI.Teal, !collected, 23);
            }
            // Accessibility fallback: after three hints, a blocked or unclickable
            // object can be collected without resetting progress or bypassing gates.
            bool canAssist = !string.IsNullOrEmpty(selectedTarget) &&
                targets.Any(p => p.id == selectedTarget) && !progress.Has(selectedTarget) &&
                hints.TryGetValue(selectedTarget, out int hintCount) && hintCount >= 3;
            ui.Button("Voltar", 242, 674, canAssist ? 468 : 956, 62, ShowPause);
            if (canAssist)
                ui.Button("Recolher com ajuda", 730, 674, 468, 62, () =>
                {
                    var id = selectedTarget;
                    ReturnToWorld();
                    if (room != null && room.Targets.TryGetValue(id, out var instance) && instance != null)
                        Collect(instance);
                    else if (progress.Collect(id))
                    {
                        Save(); DrawHUD();
                        Tell("Objeto encontrado com ajuda! Continue a charada.");
                    }
                }, HunterUI.Teal, true, 23);
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
            StopSignal(); StopNarration(); dialogBack = back; mode = Mode.Dialog; ui.Modal("Como jogar", "Entre numa maquete do quarto e explore suas portas, pontes e trilhos.");
            ui.Label("• Cada maquete esconde SEIS objetos. Leia ou ouça a charada.\n• Clique nas portas, trilhos e pontes para caminhar pelo mundo.\n• Alguns brinquedos só funcionam com uma peça encontrada.\n• Complete a charada e clique no mecanismo para usar a peça.\n• Abra atalhos e volte para observar o que mudou.\n• A câmera já fica perto dos brinquedos: basta observar.\n• Menu guarda voz, dicas e caderno. Encontre os 18 objetos.", 242, 334, 946, 294, 23);
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
            ui.Label("Kenney: Survival, Nature, Space, City, Blocky Characters\nFurniture • Toy Car • Food • Mini Characters • UI e áudio\nPato de borracha: Plat251 • CC0 • Powered by Poly Haven\nFontes: Itim • Cadson Demak / Pablo Impallari; Boogaloo • John Vargas Beltrán (OFL)\n\nInspiração de gênero: livros I Spy e I Spy: Fantasy.\nNenhum conteúdo desses livros ou jogos foi copiado.\n\nConceito solicitado por Gabriel. Código com assistência de IA.\nSons: Kenney (CC0); pássaros: isaiah658 / OpenGameArt (CC0).", 242, 338, 950, 320, 21);
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
                currentStage = map.challenge; 
                if (room != null) room.Root.SetActive(false);
                challengeView = new RoomView(map, currentStage, progress, reduceMotion);
                DrawPuzzle();
            }
            catch (Exception e) { ShowError(e); }
        }
        void DrawPuzzle()
        {
            mode = Mode.Puzzle; ui.Clear(); worldLabels.Clear(); listenButton = null;
            Color accent = RoomView.Hex(map.color);
            ui.Panel(0, 760, 1440, 140, HunterUI.Paper).raycastTarget = true;
            ui.Panel(0, 760, 1440, 3, accent);
            string intro = map.puzzle == "lock" ? "Três garrafinhas guardam a senha. Mude os números e abra o baú." :
                map.puzzle == "memory" ? "Toque no rádio da frente. Observe os brinquedos e repita os quatro sinais." :
                "Qual ferramenta ajuda cada lugar? Escolha uma peça e leve-a à miniatura.";
            ui.Label(intro, 38, 782, 1364, 40, 29, HunterUI.Ink, false, TextAnchor.MiddleCenter);
            puzzleMessage = ui.Label(map.puzzle == "lock" ? "Pistas: " + string.Join("  •  ", map.clues) :
                map.puzzle == "matching" ? (selectedSupply >= 0 ? "Na mão: " + supplies[selectedSupply] : "Praça: flores com sede. Ponte: conserto. Quartel: mensagem.") :
                "O rádio pode repetir a mensagem quando você precisar.", 38, 835, 1364, 42, 26, HunterUI.Ink, false, TextAnchor.MiddleCenter);
            ui.Button("Ajuda", 1310, 18, 108, 42, () => Help(DrawPuzzle), HunterUI.Plum, true, 21);
            CreateFeedback();
            // Only the changing lock numbers need labels; other puzzle pieces are clicked directly.
            foreach (var spot in challengeView.Hotspots.Where(h => h.Data.action == "digit"))
                WorldButton(spot, digits[spot.Data.value].ToString(), HunterUI.Plum, 66);
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
            if (!memory.Press(index)) { puzzleMessage.text = "Tente de novo. Clique no rádio para rever o sinal."; Play(wrong); return; }
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
        void OnApplicationFocus(bool focused)
        { if (!focused) { StopNarration(); if (ambience != null) ambience.Stop(); } }
        void OnApplicationPause(bool paused)
        { if (paused) { StopNarration(); if (ambience != null) ambience.Stop(); if (progress != null) Save(); } }
        void OnApplicationQuit() { if (progress != null) Save(); }
        void OnDestroy() { if (Instance == this) Instance = null; StopSignal(); StopNarration(); room?.Dispose(); shelf?.Dispose(); challengeView?.Dispose(); }
    }
}
