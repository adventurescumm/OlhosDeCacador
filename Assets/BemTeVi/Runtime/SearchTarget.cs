using System.Collections;
using UnityEngine;

namespace BemTeVi
{
    public sealed class SearchTarget : MonoBehaviour
    {
        public PropData Data { get; private set; }
        Vector3 restScale;
        Vector3 restPosition;
        Quaternion restRotation;
        bool configured;
        bool animate;
        bool collected;
        public void Configure(PropData data, bool animate)
        {
            Data = data; this.animate = animate;
            if (!configured)
            {
                restScale = transform.localScale; restPosition = transform.localPosition;
                restRotation = transform.localRotation; configured = true;
            }
            if (!animate) { StopAllCoroutines(); RestorePose(); }
        }
        public void Hover(bool value) { }
        public bool Collected => collected;
        public void RestoreCollected(bool value) { collected = value; RestorePose(); }
        public void Celebrate()
        {
            if (collected) return;
            collected = true;
            if (animate) StartCoroutine(Discovery());
        }
        IEnumerator Discovery()
        {
            float duration = .8f;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = elapsed / duration;
                if (animate)
                {
                    float arc = Mathf.Sin(t * Mathf.PI);
                    string reaction = Data.reaction;
                    Vector3 orbit = reaction == "orbit" ? new Vector3(Mathf.Sin(t * Mathf.PI * 2) * .35f, 0, (1 - Mathf.Cos(t * Mathf.PI * 2)) * .18f) : Vector3.zero;
                    transform.localPosition = restPosition + orbit + Vector3.up * arc * (reaction == "launch" ? .65f : reaction == "somersault" ? .45f : .2f);
                    transform.localRotation = restRotation * (reaction == "spin" ? Quaternion.Euler(0, t * 360, 0) :
                        reaction == "somersault" ? Quaternion.Euler(t * 360, 0, 0) :
                        reaction == "tip" ? Quaternion.Euler(0, 0, arc * 28) : Quaternion.identity);
                    transform.localScale = reaction == "squash" ? Vector3.Scale(restScale, new Vector3(1 + arc * .14f, 1 - arc * .18f, 1 + arc * .14f)) : restScale * (1 + arc * .06f);
                }
                yield return null;
            }
            RestorePose();
        }
        void RestorePose()
        {
            transform.localPosition = restPosition; transform.localRotation = restRotation;
            transform.localScale = restScale;
        }
    }
}
