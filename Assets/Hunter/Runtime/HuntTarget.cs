using System.Collections;
using UnityEngine;

namespace OlhosDeCacador
{
    public sealed class HuntTarget : MonoBehaviour
    {
        public PropData Data { get; private set; }
        Vector3 restScale;
        bool hovered;
        float hintUntil;
        bool animate;
        bool collected;
        public void Configure(PropData data, bool animate)
        {
            Data = data; restScale = transform.localScale; this.animate = animate;
        }
        public void Hover(bool value) { hovered = value; }
        public void Hint() { hintUntil = Time.unscaledTime + 4; }
        public void Celebrate()
        {
            if (collected) return;
            collected = true;
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            StartCoroutine(Discovery());
        }
        IEnumerator Discovery()
        {
            var origin = transform.localPosition; var rotation = transform.localRotation;
            float duration = animate ? .9f : .25f;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = elapsed / duration;
                if (animate)
                {
                    float arc = Mathf.Sin(t * Mathf.PI);
                    string reaction = Data.reaction;
                    transform.localPosition = origin + Vector3.up * (reaction == "launch" ? t * t * 2 : arc * .5f);
                    transform.localRotation = rotation * (reaction == "spin" ? Quaternion.Euler(0, t * 360, 0) :
                        reaction == "tip" ? Quaternion.Euler(0, 0, arc * 28) : Quaternion.identity);
                    transform.localScale = restScale * (1 + arc * .15f) * (t > .7f ? (1 - t) / .3f : 1);
                }
                yield return null;
            }
            Destroy(gameObject);
        }
        void Update()
        {
            if (collected) return;
            float hint = Time.unscaledTime < hintUntil ? (animate ? .045f * (1 + Mathf.Sin(Time.unscaledTime * 4)) : .08f) : 0;
            transform.localScale = restScale * (1 + (hovered ? .04f : 0) + hint);
        }
    }
}
