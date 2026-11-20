using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BemTeVi
{
    public sealed partial class BemTeViGame
    {
        readonly WindowsNarrator systemNarrator = new WindowsNarrator();
        readonly Queue<PropData> discoveryQueue = new Queue<PropData>();
        Coroutine discoveryVoice;
        bool NarratorSpeaking => (voice != null && voice.isPlaying) || systemNarrator.IsSpeaking;
        bool CanReadRiddle => CurrentReading() != null || WindowsNarrator.Supported;

        void ConfirmDiscovery(PropData data)
        {
            if (muted || narrationVolume <= 0) return;
            // Interrupt a long riddle once; consecutive discoveries keep their spoken order.
            if (discoveryVoice == null)
            {
                if (voice != null) voice.Stop();
                systemNarrator.Stop();
            }
            discoveryQueue.Enqueue(data);
            if (discoveryVoice == null) discoveryVoice = StartCoroutine(SpeakDiscoveries());
        }
        IEnumerator SpeakDiscoveries()
        {
            yield return null; // Ensure the coroutine handle exists even if no voice is available.
            while (discoveryQueue.Count > 0)
            {
                var item = discoveryQueue.Dequeue();
                var clip = narration != null ? narration.ForDiscovery(item.id) : null;
                if (clip != null) { voice.clip = clip; voice.Play(); }
                else systemNarrator.Speak("Você encontrou: " + item.label + ".", narrationVolume);
                while (NarratorSpeaking) yield return null;
                yield return null;
            }
            discoveryVoice = null;
        }
        void StopDiscoveryNarration()
        {
            if (discoveryVoice != null) StopCoroutine(discoveryVoice);
            discoveryVoice = null; discoveryQueue.Clear(); systemNarrator.Stop();
        }
    }
}
