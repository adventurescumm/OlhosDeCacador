using UnityEngine;

namespace OlhosDeCacador
{
    // Clickable existing props: entrances, signposts, puzzle pieces and little surprises.
    // Transform animation only; no meshes, textures, sprites or audio are synthesized.
    public sealed class WorldHotspot : MonoBehaviour
    {
        public PropData Data { get; private set; }
        Vector3 restPosition, restScale;
        Quaternion restRotation;
        bool animate, hovered;
        float reactionUntil, markedUntil;
        Vector3 travelFrom;
        float travelUntil;
        public void Configure(PropData data, bool motion)
        {
            Data = data; animate = motion;
            restPosition = transform.localPosition; restRotation = transform.localRotation;
            restScale = transform.localScale;
        }
        public void Hover(bool value) { hovered = value; }
        public void React() { reactionUntil = Time.unscaledTime + .8f; }
        public void PlaceAt(Vector3 position)
        {
            travelFrom = transform.localPosition; restPosition = position;
            travelUntil = animate ? Time.unscaledTime + .6f : 0;
        }
        public void Mark(float seconds = 1) { markedUntil = Time.unscaledTime + seconds; }
        void Update()
        {
            float t = Mathf.Clamp01((reactionUntil - Time.unscaledTime) / .8f);
            float wave = animate ? Mathf.Sin(t * Mathf.PI) : 0;
            float travel = Mathf.Clamp01((travelUntil - Time.unscaledTime) / .6f);
            transform.localPosition = Vector3.Lerp(restPosition, travelFrom, travel) + Vector3.up *
                (wave * (Data.reaction == "launch" ? 1.4f : .18f) + Mathf.Sin(travel * Mathf.PI) * .5f);
            transform.localRotation = restRotation * (Data.reaction == "spin" ? Quaternion.Euler(0, wave * 30, 0) :
                Quaternion.Euler(0, 0, wave * Mathf.Sin(t * 12) * 10));
            transform.localScale = restScale * (1 + (hovered ? .025f : 0) + (Time.unscaledTime < markedUntil ? .10f : 0));
        }
    }
}
