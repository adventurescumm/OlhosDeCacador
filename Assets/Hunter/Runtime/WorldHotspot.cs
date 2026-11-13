using UnityEngine;

namespace OlhosDeCacador
{
    // Clickable existing props: entrances, signposts, puzzle pieces and little surprises.
    // Transform animation only; no meshes, textures, sprites or audio are synthesized.
    public sealed class WorldHotspot : MonoBehaviour
    {
        public PropData Data { get; private set; }
        Vector3 restPosition, restScale, closedPosition;
        Quaternion restRotation, closedRotation, travelRotation;
        bool animate, hovered, configured;
        float reactionUntil, markedUntil;
        Vector3 travelFrom;
        float travelUntil;
        public void Configure(PropData data, bool motion)
        {
            Data = data; animate = motion;
            if (configured)
            {
                if (!motion) { reactionUntil = 0; travelUntil = 0; transform.localPosition = restPosition; transform.localRotation = restRotation; }
                return;
            }
            configured = true;
            restPosition = transform.localPosition; restRotation = transform.localRotation;
            restScale = transform.localScale; closedPosition = restPosition; closedRotation = restRotation;
            travelRotation = restRotation;
        }
        public void Hover(bool value) { hovered = value; }
        public void React() { reactionUntil = Time.unscaledTime + .8f; }
        public void PlaceAt(Vector3 position)
        {
            travelFrom = transform.localPosition; travelRotation = transform.localRotation; restPosition = position;
            travelUntil = animate ? Time.unscaledTime + .6f : 0;
        }
        public void SetOpen(bool opened, bool motion)
        {
            var offset = Data.openedOffset;
            var rotation = Data.openedRotation;
            travelFrom = transform.localPosition; travelRotation = transform.localRotation;
            restPosition = closedPosition + (opened && offset != null && offset.Length == 3 ? new Vector3(offset[0], offset[1], offset[2]) : Vector3.zero);
            restRotation = closedRotation * (opened && rotation != null && rotation.Length == 3 ? Quaternion.Euler(rotation[0], rotation[1], rotation[2]) : Quaternion.identity);
            travelUntil = motion && animate ? Time.unscaledTime + .6f : 0;
            if (travelUntil == 0) { transform.localPosition = restPosition; transform.localRotation = restRotation; }
        }
        public void Mark(float seconds = 1) { markedUntil = Time.unscaledTime + seconds; }
        void Update()
        {
            float t = Mathf.Clamp01((reactionUntil - Time.unscaledTime) / .8f);
            float wave = animate ? Mathf.Sin(t * Mathf.PI) : 0;
            float travel = Mathf.Clamp01((travelUntil - Time.unscaledTime) / .6f);
            transform.localPosition = Vector3.Lerp(restPosition, travelFrom, travel) + Vector3.up *
                (wave * (Data.reaction == "launch" ? 1.4f : .18f) + Mathf.Sin(travel * Mathf.PI) * .5f);
            transform.localRotation = Quaternion.Slerp(restRotation, travelRotation, travel) * (Data.reaction == "spin" ? Quaternion.Euler(0, wave * 30, 0) :
                Quaternion.Euler(0, 0, wave * Mathf.Sin(t * 12) * 10));
            transform.localScale = restScale * (1 + (hovered ? .025f : 0) + (Time.unscaledTime < markedUntil ? .10f : 0));
        }
    }
}
