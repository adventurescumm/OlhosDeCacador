using UnityEngine;

namespace BemTeVi
{
    // One marker per .unity scene. Its shot and prop transforms are editable in the Inspector.
    public sealed class BemTeViStage : MonoBehaviour
    {
        public static BemTeViStage Active { get; private set; }
        public string stageId;
        public Vector3 cameraPosition;
        public Vector3 lookAt;
        [Range(20, 60)] public float fieldOfView = 40;
        public const int ContentVersion = 12;
        public int contentVersion;
        public bool baked;
        public GameObject contentRoot;
        void Awake()
        {
            if (!Application.isPlaying) return;
            Active = this;
            if (contentVersion != ContentVersion && contentRoot != null)
            {
                // The catalog migration retains all existing placements and adds the new toys.
                contentRoot.SetActive(false); Destroy(contentRoot); contentRoot = null;
            }
            if (BemTeViGame.Instance == null) new GameObject("Sessão persistente — Bem-Te-Vi").AddComponent<BemTeViGame>();
        }
        void OnDestroy() { if (Active == this) Active = null; }
    }
}
