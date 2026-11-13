using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OlhosDeCacador
{
    public sealed partial class HunterGame
    {
        public static HunterGame Instance { get; private set; }
        bool loadingScene;
        void Awake()
        {
            if (!Application.isPlaying) return;
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }
        bool RouteTo(string sceneName, Action ready, bool reload = false)
        {
            if (loadingScene) return true;
            if (!reload && SceneManager.GetActiveScene().name == sceneName) return false;
            StartCoroutine(ChangeScene(sceneName, ready)); return true;
        }
        IEnumerator ChangeScene(string sceneName, Action ready)
        {
            loadingScene = true; mode = Mode.Loading;
            StopSignal(); StopNarration(); ClearWorldHover(); worldLabels.Clear();
            room?.Dispose(); room = null; CloseChallenge(); shelf?.Dispose(); shelf = null;
            ui.Clear(); ui.Panel(0, 0, 1440, 900, HunterUI.Paper);
            ui.Label("Abrindo outro cantinho…", 300, 410, 840, 80, 38, HunterUI.Plum, true, TextAnchor.MiddleCenter);
            AsyncOperation operation;
            try { operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single); }
            catch (Exception e) { loadingScene = false; ShowError(e); yield break; }
            if (operation == null) { loadingScene = false; ShowError(new InvalidOperationException("Cena não carregada: " + sceneName)); yield break; }
            while (!operation.isDone) yield return null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.42f, .44f, .52f);
            RenderSettings.skybox = null;
            loadingScene = false; ready();
        }
        void StartAtScene()
        {
            string id = HunterStage.Active != null ? HunterStage.Active.stageId : "inicio";
            if (id == "inicio") { ShowTitle(); return; }
            if (id == "quarto") { ShowMenu(); return; }
            map = catalog.maps.FirstOrDefault(m => m.junction.id == id || m.challenge.id == id || m.rooms.Any(r => r.id == id));
            if (map == null) { ShowTitle(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == id);
            if (roomIndex >= 0) { LoadRoom(); return; }
            if (id == map.challenge.id && progress.Ready(map) && map.gates.All(g => progress.Opened(g.id)) && !progress.Solved(map))
            { roomIndex = map.rooms.Length - 1; OpenPuzzle(); }
            else ShowJunction();
        }
        void RestartSession()
        {
            Instance = null; Destroy(gameObject); SceneManager.LoadScene("Main");
        }
    }
}
