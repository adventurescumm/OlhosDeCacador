using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BemTeVi
{
    public sealed partial class BemTeViGame
    {
        public static BemTeViGame Instance { get; private set; }
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
            ui.Clear();
            var curtain = ui.Panel(0, 0, 1440, 900, new Color(.13f, .10f, .18f, 0));
            curtain.raycastTarget = true;
            if (!reduceMotion)
                for (float elapsed = 0; elapsed < .16f; elapsed += Time.unscaledDeltaTime)
                { curtain.color = new Color(.13f, .10f, .18f, elapsed / .16f); yield return null; }
            curtain.color = new Color(.13f, .10f, .18f, 1);
            room?.Dispose(); room = null; CloseChallenge(); shelf?.Dispose(); shelf = null;
            AsyncOperation operation;
            try { operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single); }
            catch (Exception e) { loadingScene = false; ShowError(e); yield break; }
            if (operation == null) { loadingScene = false; ShowError(new InvalidOperationException("Cena não carregada: " + sceneName)); yield break; }
            while (!operation.isDone) yield return null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.42f, .44f, .52f);
            RenderSettings.skybox = null;
            loadingScene = false; ready();
            curtain = ui.Panel(0, 0, 1440, 900, new Color(.13f, .10f, .18f, 1));
            curtain.raycastTarget = true;
            if (!reduceMotion)
                for (float elapsed = 0; elapsed < .2f; elapsed += Time.unscaledDeltaTime)
                {
                    if (curtain == null) yield break;
                    curtain.color = new Color(.13f, .10f, .18f, 1-elapsed/.2f); yield return null;
                }
            if (curtain != null) Destroy(curtain.gameObject);
        }
        void StartAtScene()
        {
            string id = BemTeViStage.Active != null ? BemTeViStage.Active.stageId : "inicio";
            if (activePlayer == null) { ShowTitle(); return; }
            if (id == "inicio") { ShowTitle(); return; }
            if (id == "quarto") { ShowMenu(); return; }
            map = catalog.maps.FirstOrDefault(m => m.junction.id == id || m.challenge.id == id || m.rooms.Any(r => r.id == id));
            if (map == null) { ShowTitle(); return; }
            roomIndex = Array.FindIndex(map.rooms, r => r.id == id);
            if (roomIndex >= 0) { LoadRoom(); return; }
            if (id == map.challenge.id)
            { roomIndex = map.rooms.Length - 1; OpenPuzzle(); }
            else ShowJunction();
        }
        void RestartSession()
        {
            Instance = null; Destroy(gameObject); SceneManager.LoadScene("Main");
        }
    }
}
