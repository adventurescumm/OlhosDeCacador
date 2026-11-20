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

namespace BemTeVi
{
    public sealed partial class BemTeViGame : MonoBehaviour
    {
        enum Mode { Loading, Title, Menu, Junction, Explore, Pause, Options, Puzzle, Dialog, Profiles, Victory, Error }
        const string SaveKey = "BemTeVi.v1";
        Catalog catalog;
        Progress progress;
        BemTeViUI ui;
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
        SearchTarget hovered;
        Text message, hoverText, puzzleMessage;
        AudioSource sound;
        AudioClip click, found, wrong;
        AudioClip[] notes;
        string saveWarning = "";
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
            ui = new BemTeViUI(transform);
            // The package assigns its default UI actions when this module is enabled.
            var events = new GameObject("Eventos", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform);
            pointerData = new PointerEventData(events.GetComponent<EventSystem>());
            try
            {
                if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                    throw new InvalidOperationException("URP não está ativo. Execute Bem-Te-Vi > 1. Preparar projeto no editor.");
                Application.targetFrameRate = 60;
                muted = PlayerPrefs.GetInt(SaveKey + ".mute", 0) == 1;
                reduceMotion = PlayerPrefs.GetInt(SaveKey + ".motion", 0) == 1;
                autoNarration = PlayerPrefs.GetInt(SaveKey + ".autoVoice", 0) == 1;
                effectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SaveKey + ".effectsVolume", .5f));
                narrationVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SaveKey + ".voiceVolume", 1f));
                if (narration == null) narration = Resources.Load<NarrationBank>("BemTeVi/Narration/NarrationBank");
                var data = Resources.Load<TextAsset>("BemTeVi/catalog");
                if (data == null) throw new InvalidOperationException("Catálogo não encontrado.");
                catalog = JsonUtility.FromJson<Catalog>(data.text);
                if (catalog?.maps == null || catalog.maps.Length != 3) throw new InvalidOperationException("Catálogo incompleto.");
                LoadPlayers();
                var background = new GameObject("Fundo", typeof(Camera)).GetComponent<Camera>();
                background.transform.SetParent(transform); background.depth = -10; background.cullingMask = 0;
                background.clearFlags = CameraClearFlags.SolidColor; background.backgroundColor = BemTeViUI.Paper;
                ConfigureCamera(background, false);
                worldCamera = new GameObject("Câmera do diorama", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                worldCamera.transform.SetParent(transform); worldCamera.tag = "MainCamera";
                worldCamera.clearFlags = CameraClearFlags.SolidColor; worldCamera.backgroundColor = BemTeViUI.Paper;
                worldCamera.fieldOfView = 43; worldCamera.nearClipPlane = .1f; worldCamera.farClipPlane = 60;
                ConfigureCamera(worldCamera, true);
                var light = new GameObject("Luz do quarto", typeof(Light)).GetComponent<Light>();
                light.transform.SetParent(transform); light.type = LightType.Directional; light.intensity = .95f;
                light.transform.rotation = Quaternion.Euler(45, -35, 0); light.shadows = LightShadows.Soft;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .44f, .52f);
                light.color = RoomView.Hex("#FFF0D9"); light.intensity = 1.2f; light.shadowStrength = .65f;
                // Shadows and MSAA are configured in BemTeViURP.asset, as required by URP.
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
        AudioClip Clip(string name) => Resources.Load<AudioClip>("BemTeVi/Audio/" + name);
        void Play(AudioClip clip) { if (!muted && clip != null && sound != null) sound.PlayOneShot(clip, NarratorSpeaking ? .35f : .65f); }
        void Tap() => Play(click);
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
            if (listenButton != null) listenButton.GetComponentInChildren<Text>().text = NarratorSpeaking ? "Parar leitura" : "Ouvir charada";
            if (ui.LastActionFrame == Time.frameCount) { ClearWorldHover(); return; }
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                if (mode == Mode.Explore || mode == Mode.Junction) ShowPause();
                else if (mode == Mode.Options) optionsBack?.Invoke();
                else if (mode == Mode.Dialog) (dialogBack ?? ReturnToWorld).Invoke();
                else if (mode == Mode.Pause || mode == Mode.Puzzle) ReturnToWorld();
                else if (mode == Mode.Menu) ShowTitle();
                else if (mode == Mode.Profiles) ShowTitle();
                else if (mode == Mode.Victory) ShowJunction();
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
        void SetHover(SearchTarget target)
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
        void Collect(SearchTarget target)
        {
            var data = target.Data;
            if (!progress.Collect(data.id)) return;
            SetHover(null); target.Celebrate();
            Save(); DrawHUD(); ConfirmDiscovery(data); PlayObjectSound(data, true);
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
            ui.Label("BEM-TE-VI", 94, 150, 562, 120, 88, RoomView.Hex("#FFCF79"), true, TextAnchor.MiddleCenter);
            ui.Label("Um quarto cheio de mundos para descobrir.", 108, 312, 532, 68, 24, Color.white, false, TextAnchor.MiddleCenter);
            var play = ui.Button("Iniciar", 158, 488, 430, 82, BeginAdventure, RoomView.Hex("#B44264"), true, 37);
            titlePlay = play.GetComponent<RectTransform>();
            ui.Button("Opções", 158, 592, 430, 66, () => ShowOptions(ShowTitle), RoomView.Hex("#247B91"), true, 29);
            ui.Button("Como jogar", 158, 676, 430, 66, () => Help(ShowTitle), RoomView.Hex("#6F4D9C"), true, 29);
            TitleStar(622, 62, 48); TitleStar(98, 405, 35);
            ui.Label("Enter abre seu caderno de aventuras", 126, 770, 502, 32, 18, BemTeViUI.Ink, false, TextAnchor.MiddleCenter);
            if (!string.IsNullOrEmpty(saveWarning)) ui.Label(saveWarning, 710, 808, 670, 62, 20, BemTeViUI.Ink);
        }

        void TitleStar(float x, float y, float size)
        {
            titleStars.Add(ui.Art("minimap_icon_star_yellow", x, y, size, size).rectTransform);
        }
        void BeginAdventure()
        {
            // A submit event and the Enter shortcut can arrive in the same frame.
            if (mode != Mode.Title) return;
            Tap(); ShowPlayers();
        }
        void ShowMenu()
        {
            if (RouteTo("quarto", ShowMenu, room != null)) return;
            if (!PrepareBedroom(Mode.Menu)) return;
            ui.Clear();
            ui.Label("Escolha uma brincadeira", 32, 22, 670, 46, 32, BemTeViUI.Paper, true);
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, BemTeViUI.Plum, true, 21);
            ui.Button("Início", 1188, 18, 108, 42, ShowTitle, BemTeViUI.Teal, true, 21);
            CreateFeedback(); RememberStage();
        }

        void EnterMap(MapData selected)
        {
            Tap(); map = selected; roomIndex = -1;
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
                RememberStage(); if (autoNarration) PlayNarration(); return true;
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
            var targets = currentRoom.Targets.ToArray();
            // The lower band belongs only to the riddle, as in a picture-book search.
            ui.Panel(0, 760, 1440, 140, BemTeViUI.Paper).raycastTarget = true;
            ui.Panel(0, 760, 1440, 3, RoomView.Hex(currentStage.color));
            string[] lines = currentRoom.Riddle.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                bool collected = i < targets.Length && progress.Has(targets[i].id);
                string line = (collected ? "• " : "") + lines[i];
                ui.Label(line, i < 3 ? 38 : 742, 778 + (i % 3) * 36, 660, 35, 27,
                    collected ? BemTeViUI.Teal : BemTeViUI.Ink);
            }
            ui.Button("Menu", 1310, 18, 108, 42, ShowPause, BemTeViUI.Plum, true, 21);
            CreateFeedback(); message = null;
            if (!string.IsNullOrEmpty(saveWarning)) Tell(saveWarning);
        }

        void ShowPause()
        {
            StopNarration(); Tap(); mode = Mode.Pause; SetHover(null);
            ui.Modal("Uma pausa na brincadeira", map == null ? "O quarto guarda suas três aventuras." :
                map.title + " • Rodada " + (map.currentRound + 1) + "/3 • " + progress.Count(map) + " de 18 descobertas");
            ui.Button("Continuar", 242, 342, 956, 56, ReturnToWorld, BemTeViUI.Teal, true, 27);
            bool searching = map != null && roomIndex >= 0 && currentStage == map.rooms[roomIndex];
            if (searching)
            {
                ui.Button("Ouvir charada", 242, 414, 468, 54, () => { ReturnToWorld(); if (muted || narrationVolume <= 0) Tell("A voz está desligada. Ajuste o volume em Menu > Opções."); else PlayNarration(); }, BemTeViUI.Plum, CanReadRiddle);
                ui.Button("Ler juntos", 730, 414, 468, 54, ShowReading, BemTeViUI.Plum);
                ui.Button("Caderno de descobertas", 242, 482, 956, 54, Notebook, BemTeViUI.Teal);
            }
            else
            {
                ui.Button("Como jogar", 242, 414, 468, 54, () => Help(ShowPause));
                ui.Button("Créditos", 730, 414, 468, 54, () => Credits(ShowPause));
                ui.Label("As portas, pontes e trilhos levam aos próximos cantinhos.", 242, 490, 956, 48, 25);
            }
            ui.Button("Opções", 242, 550, 468, 54, () => ShowOptions(ShowPause), BemTeViUI.Plum);
            ui.Button("Como jogar", 730, 550, 468, 54, () => Help(ShowPause), BemTeViUI.Plum);
            ui.Button(map == null ? "Nova brincadeira" : "Recomeçar este mundo", 242, 618, 468, 54,
                () => Confirm("Recomeçar?", map == null ? "Isso limpa o progresso dos três mundos." : "Os outros mundos continuam guardados.",
                    () => { if (map == null) { progress.ResetAll(); activePlayer.puzzles.Clear(); Save(); ShowMenu(); }
                        else { progress.ResetMap(map); activePlayer.puzzles.RemoveAll(p => p.mapId == map.id); roomIndex = -1; Save(); ShowJunction(); } }, ShowPause));
            ui.Button("Sair do jogo", 730, 618, 468, 54, () => ConfirmExit(ShowPause));
            ui.Button("Trocar jogador", 242, 690, 956, 54, () => { Save(); ShowPlayers(); }, BemTeViUI.Teal);
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
            ui.Label("SONS DOS BRINQUEDOS", 196, 238, 650, 34, 24, BemTeViUI.Teal, true);
            VolumeRow(292, effectsVolume, value => { effectsVolume = value; ApplyAudioSettings(); Save(); Play(click); ShowOptions(back); });
            ui.Label("VOZ DO NARRADOR", 196, 382, 650, 34, 24, BemTeViUI.Plum, true);
            VolumeRow(436, narrationVolume, value => { narrationVolume = value; ApplyAudioSettings(); Save(); ShowOptions(back); });
            ui.Button(muted ? "Áudio desligado" : "Áudio ligado", 916, 264, 324, 62,
                () => { muted = !muted; ApplyAudioSettings(); Save(); ShowOptions(back); }, BemTeViUI.Teal);
            ui.Button(autoNarration ? "Leitura automática: sim" : "Leitura automática: não", 916, 418, 324, 78,
                () => { autoNarration = !autoNarration; Save(); ShowOptions(back); }, BemTeViUI.Plum, true, 20);
            ui.Label("A leitura automática usa a voz gravada ao entrar em cada cantinho.", 196, 541, 1044, 50, 21, BemTeViUI.Muted);
            ui.Button(reduceMotion ? "Movimento reduzido" : "Movimento suave", 196, 622, 490, 64,
                () => { reduceMotion = !reduceMotion; Save(); ShowOptions(back); }, BemTeViUI.Teal);
            ui.Label("Reduz o movimento das reações dos brinquedos.", 722, 622, 510, 72, 22, BemTeViUI.Muted);
            ui.Button("Pronto! Voltar à brincadeira", 420, 782, 600, 68, back, BemTeViUI.Plum, true, 25);
        }
        void VolumeRow(float y, float value, Action<float> changed)
        {
            ui.Button("−", 196, y, 66, 56, () => changed(Mathf.Clamp01(value - .1f)), BemTeViUI.Ink, value > 0, 30);
            for (int i = 0; i < 10; i++) ui.Panel(282 + i * 42, y + 10, 32, 36, value >= (i + 1) * .1f - .001f ? BemTeViUI.Teal : new Color(.76f, .72f, .62f));
            ui.Button("+", 720, y, 66, 56, () => changed(Mathf.Clamp01(value + .1f)), BemTeViUI.Ink, value < 1, 30);
            ui.Label(Mathf.RoundToInt(value * 100) + "%", 800, y + 12, 100, 36, 24, BemTeViUI.Ink, true);
        }
        AudioClip CurrentReading() => map != null && roomIndex >= 0 && narration != null ? narration.ForRoom(map.rooms[roomIndex].id, map.currentRound) : null;
        void StopNarration()
        {
            if (voice != null) voice.Stop();
            StopDiscoveryNarration();
        }
        void PlayNarration()
        {
            var clip = CurrentReading();
            if (voice == null || muted || narrationVolume <= 0) return;
            StopNarration();
            if (clip != null) { voice.clip = clip; voice.Play(); }
            else if (map != null && roomIndex >= 0) systemNarrator.Speak(map.rooms[roomIndex].Riddle, narrationVolume);
        }
        void ToggleNarration()
        {
            if (NarratorSpeaking) { StopNarration(); return; }
            if (muted || narrationVolume <= 0) { Tell("O áudio está desligado. Você pode ajustá-lo em Pausa > Opções."); return; }
            PlayNarration();
        }
        void ShowReading()
        {
            StopNarration(); dialogBack = ReturnToWorld; mode = Mode.Dialog;
            ui.Modal("Vamos ler juntos?", map.rooms[roomIndex].name + " • Rodada " + (map.currentRound + 1) + " • Uma pessoa lê a charada; a turma procura os tesouros.");
            ui.Label(map.rooms[roomIndex].Riddle, 244, 350, 948, 244, 33, BemTeViUI.Ink, false, TextAnchor.MiddleLeft);
            ui.Button("Voltar e procurar", 242, 674, 956, 62, ReturnToWorld, RoomView.Hex(map.color));
        }
        void Help(Action back)
        {
            StopSignal(); StopNarration(); dialogBack = back; mode = Mode.Dialog; ui.Modal("Como jogar", "Entre numa maquete do quarto e explore suas portas, pontes e trilhos.");
            ui.Label("• Cada maquete esconde SEIS objetos. Leia ou ouça a charada.\n• Clique nas portas, trilhos e pontes para caminhar pelo mundo.\n• Alguns brinquedos só funcionam com uma peça encontrada.\n• Você pode sair e tentar outra sala a qualquer momento.\n• Abra atalhos e volte para observar o que mudou.\n• A câmera já fica perto dos brinquedos: basta observar.\n• São três rodadas por mundo, cada uma com 18 novos alvos.", 242, 334, 946, 294, 23);
            ui.Button("Entendi!", 242, 674, 956, 62, back);
        }
        void Notebook()
        {
            StopNarration(); dialogBack = ReturnToWorld; Tap(); mode = Mode.Dialog; ui.Modal("Caderno de descobertas", "As pistas encontradas ficam aqui. Cada mundo guarda sua própria brincadeira.");
            var clues = map.Targets.Where(p => progress.Has(p.id) && !string.IsNullOrEmpty(p.clue)).Select(p => "• " + p.clue).ToArray();
            ui.Label(clues.Length == 0 ? "Ainda não há anotações. Explore os objetos das três telas!" : string.Join("\n\n", clues), 242, 348, 948, 276, 27);
            ui.Button("Voltar à busca", 242, 674, 956, 62, ReturnToWorld);
        }
        void Credits(Action back)
        {
            StopNarration(); dialogBack = back; mode = Mode.Dialog; ui.Modal("Feito para brincar", "Assets prontos de Kenney e Poly Haven; licenças nos arquivos do projeto.");
            ui.Label("Kenney: Survival, Nature, Space, City, Blocky Characters\nFurniture • Toy Car • Food • Mini Characters • Holiday, Train e Brick Kits • UI e áudio\nPato de borracha: Plat251 • CC0 • Powered by Poly Haven\nFontes: Itim • Cadson Demak / Pablo Impallari; Boogaloo • John Vargas Beltrán (OFL)\n\nInspiração de gênero: livros I Spy e I Spy: Fantasy.\nNenhum conteúdo desses livros ou jogos foi copiado.\n\nConceito solicitado por Gabriel. Código com assistência de IA.\nSons: Kenney (CC0); pássaros: isaiah658 / OpenGameArt (CC0).", 242, 338, 950, 320, 21);
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
            if (RouteTo(map.challenge.id, OpenPuzzle)) return;
            StopNarration(); StopSignal(); ClearWorldHover(); Tap();
            digits = new int[3]; assignments = new[] { -1, -1, -1 }; selectedSupply = -1;
            memory = new MemorySequence(map.Solution);
            try
            {
                currentStage = map.challenge;
                if (room != null) room.Root.SetActive(false);
                challengeView = new RoomView(map, currentStage, progress, reduceMotion);
                RestorePuzzle(); DrawPuzzle(); RememberStage();
            }
            catch (Exception e) { ShowError(e); }
        }
        void DrawPuzzle()
        {
            mode = Mode.Puzzle; ui.Clear(); worldLabels.Clear(); listenButton = null;
            Color accent = RoomView.Hex(map.color);
            ui.Panel(0, 760, 1440, 140, BemTeViUI.Paper).raycastTarget = true;
            ui.Panel(0, 760, 1440, 3, accent);
            string intro = map.puzzle == "lock" ? "Três garrafinhas guardam a senha. Mude os números e abra o baú." :
                map.puzzle == "memory" ? "Toque no rádio da frente. Observe os brinquedos e repita os quatro sinais." :
                "Qual ferramenta ajuda cada lugar? Escolha uma peça e leve-a à miniatura.";
            ui.Label(map.PuzzleIntro, 38, 782, 1364, 40, 29, BemTeViUI.Ink, false, TextAnchor.MiddleCenter);
            puzzleMessage = ui.Label(map.puzzle == "lock" ? "Pistas: " + string.Join("  •  ", map.Targets.Where(p => progress.Has(p.id) && !string.IsNullOrEmpty(p.clue)).Select(p => p.clue)) :
                map.puzzle == "matching" ? (selectedSupply >= 0 ? "Na mão: " + supplies[selectedSupply] : string.Join("  •  ", map.Clues)) :
                "O rádio pode repetir a mensagem quando você precisar.", 38, 835, 1364, 42, 26, BemTeViUI.Ink, false, TextAnchor.MiddleCenter);
            ui.Button("Ajuda", 1310, 18, 108, 42, () => Help(DrawPuzzle), BemTeViUI.Plum, true, 21);
            CreateFeedback();
            // Only the changing lock numbers need labels; other puzzle pieces are clicked directly.
            foreach (var spot in challengeView.Hotspots.Where(h => h.Data.action == "digit"))
                WorldButton(spot, digits[spot.Data.value].ToString(), BemTeViUI.Plum, 66);
        }
        IEnumerator ShowSignal()
        {
            playingSignal = true; memory.Reset();
            for (int i = 0; i < map.Solution.Length; i++)
            {
                int symbol = map.Solution[i];
                puzzleMessage.text = "Sinal " + (i + 1) + " de " + map.Solution.Length + ": " + symbols[symbol];
                var spot = challengeView.Hotspots.First(h => h.Data.action == "signal" && h.Data.value == symbol);
                spot.Mark(.85f); if (!reduceMotion) spot.React();
                Play(notes[symbol]); yield return new WaitForSecondsRealtime(.9f);
                puzzleMessage.text = "Observe..."; yield return new WaitForSecondsRealtime(.25f);
            }
            playingSignal = false; signal = null; puzzleMessage.text = "Sua vez! Repita os " + map.Solution.Length + " sinais.";
        }
        void PressSignal(int index)
        {
            if (playingSignal) return;
            Play(notes[index]);
            if (!memory.Press(index)) { puzzleMessage.text = "Tente de novo. Clique no rádio para rever o sinal."; Play(wrong); return; }
            puzzleMessage.text = "Muito bem! " + memory.Position + " de " + map.Solution.Length + " sinais.";
            if (memory.Complete) Win();
        }
        void Match(int destination)
        {
            if (selectedSupply < 0) { puzzleMessage.text = "Primeiro escolha uma ferramenta."; return; }
            if (assignments[destination] >= 0) return;
            if (!PuzzleRules.Match(selectedSupply, destination, map.Solution))
            { Play(wrong); puzzleMessage.text = "Pense no que esse lugar precisa e tente outra combinação."; return; }
            var item = challengeView.Hotspots.First(h => h.Data.action == "supply" && h.Data.value == selectedSupply);
            var place = challengeView.Hotspots.First(h => h.Data.action == "destination" && h.Data.value == destination);
            item.PlaceAt(place.transform.localPosition + new Vector3(.7f, 0, -.6f)); place.React();
            assignments[destination] = selectedSupply; selectedSupply = -1; SavePuzzle(); Play(found);
            if (assignments.All(a => a >= 0)) Win(); else DrawPuzzle();
        }
        void Win()
        {
            if (!progress.Ready(map))
            {
                puzzleMessage.text = "O mecanismo funcionou! Ainda faltam " + (18 - progress.Count(map)) + " descobertas nesta rodada. Explore as outras salas e volte.";
                memory.Reset(); assignments = new[] { -1, -1, -1 }; selectedSupply = -1;
                foreach (var item in challengeView.Hotspots.Where(h => h.Data.action == "supply"))
                    item.PlaceAt(new Vector3(item.Data.x, item.Data.y, item.Data.z));
                SavePuzzle();
                return;
            }
            StopSignal(); StopNarration(); ClearWorldHover();
            int completedRound = map.currentRound + 1;
            if (!progress.Complete(map)) return;
            activePlayer.stageId = map.junction.id; Save(); Play(found); mode = Mode.Victory;
            foreach (var spot in challengeView.Hotspots) spot.React();
            ui.Clear(); worldLabels.Clear();
            ui.Panel(0, 0, 1440, 96, BemTeViUI.Plum);
            ui.Label(progress.Finished ? "BEM-TE-VI! NOVE BRINCADEIRAS COMPLETAS!" : "RODADA " + completedRound + " COMPLETA!", 100, 24, 1240, 56, 38, BemTeViUI.Paper, true, TextAnchor.MiddleCenter);
            ui.Art("minimap_icon_star_yellow", 32, 20, 60, 60);
            ui.Art("minimap_icon_star_yellow", 1348, 20, 60, 60);
            ui.Panel(0, 724, 1440, 176, BemTeViUI.Plum);
            string badge = map.id == "faroeste" ? "XERIFE DA BRINCADEIRA" : map.id == "espaco" ? "EXPLORADOR LUNAR" : "HERÓI DO QUINTAL";
            ui.Label(badge + (progress.Finished ? " • OS NOVE SELOS SÃO SEUS!" : " • SELO " + completedRound + " CONQUISTADO"), 80, 742, 1280, 42, 30, BemTeViUI.Paper, true, TextAnchor.MiddleCenter);
            ui.Label(progress.Solved(map) ? "As três rodadas deste mundo estão completas!" : "Os brinquedos guardam seis novos versos em cada sala!", 280, 814, 420, 62, 21, BemTeViUI.Paper);
            ui.Button(progress.Solved(map) ? "Voltar ao quarto" : "Próxima rodada", 740, 814, 420, 62, () => { roomIndex = -1; if (progress.Solved(map)) ShowMenu(); else ShowJunction(); }, BemTeViUI.Teal, true, 23);
        }

        void ShowError(Exception exception)
        {
            Debug.LogException(exception); StopSignal(); StopNarration(); mode = Mode.Error;
            ui.Modal("Uma peça não carregou", "Confira se todos os arquivos do projeto foram importados. Suas descobertas anteriores continuam salvas.");
            ui.Label(exception.Message, 242, 352, 950, 180, 24);
            ui.Button("Tentar novamente", 242, 674, 956, 62, RestartSession);
        }
        void OnApplicationFocus(bool focused)
        { if (!focused) { StopNarration(); if (ambience != null) ambience.Stop(); if (progress != null) Save(); } }
        void OnApplicationPause(bool paused)
        { if (paused) { StopNarration(); if (ambience != null) ambience.Stop(); if (progress != null) Save(); } }
        void OnApplicationQuit() { if (progress != null) Save(); }
        void OnDestroy() { if (Instance == this) Instance = null; StopSignal(); StopNarration(); systemNarrator.Dispose(); room?.Dispose(); shelf?.Dispose(); challengeView?.Dispose(); }
    }
}
