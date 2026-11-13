using UnityEngine;

namespace OlhosDeCacador
{
    // One marker per .unity scene. Its shot and prop transforms are editable in the Inspector.
    public sealed class HunterStage : MonoBehaviour
    {
        public static HunterStage Active { get; private set; }
        public string stageId;
        public Vector3 cameraPosition;
        public Vector3 lookAt;
        [Range(20, 60)] public float fieldOfView = 40;
        public bool baked;
        public GameObject contentRoot;
        void Awake()
        {
            if (!Application.isPlaying) return;
            Active = this;
            if (HunterGame.Instance == null) new GameObject("Sessão persistente — Olhos de Caçador").AddComponent<HunterGame>();
        }
        void OnDestroy() { if (Active == this) Active = null; }
    }
}
