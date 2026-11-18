using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OlhosDeCacador
{
    [Serializable] public sealed class SoundCatalog { public StageSounds[] stages; }
    [Serializable] public sealed class StageSounds
    {
        public string id, passage, mechanism, discovery, ambient;
        public float ambientGain = .14f, ambientGap = 12;
    }

    public sealed partial class HunterGame
    {
        SoundCatalog soundCatalog;
        StageSounds stageSounds;
        AudioSource ambience;
        string audioStage;
        float nextAmbientTime;
        readonly Dictionary<string, AudioClip> effectClips = new Dictionary<string, AudioClip>();

        void InitializeRoomAudio()
        {
            var text = Resources.Load<TextAsset>("Hunter/soundscapes");
            if (text == null) throw new InvalidOperationException("Catálogo de sons ausente: Hunter/soundscapes.");
            soundCatalog = JsonUtility.FromJson<SoundCatalog>(text.text);
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.playOnAwake = false; ambience.loop = false; ambience.spatialBlend = 0;
        }
        AudioClip Effect(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (!effectClips.TryGetValue(name, out var clip))
            { clip = Clip(name); effectClips[name] = clip; }
            return clip;
        }
        void UpdateRoomAudio()
        {
            if (ambience == null) return;
            bool exploring = mode == Mode.Menu || mode == Mode.Junction || mode == Mode.Explore || mode == Mode.Puzzle;
            if (!exploring || muted || effectsVolume <= 0 || !Application.isFocused)
            { ambience.Stop(); return; }
            if (currentStage == null) return;
            if (audioStage != currentStage.id)
            {
                ambience.Stop(); audioStage = currentStage.id;
                stageSounds = soundCatalog.stages.FirstOrDefault(s => s.id == audioStage);
                ambience.clip = Effect(stageSounds?.ambient);
                nextAmbientTime = Time.unscaledTime + 1.5f;
            }
            // Quiet original recordings with pauses between repeats; no synthetic loops.
            ambience.volume = effectsVolume * (stageSounds?.ambientGain ?? 0) *
                (voice != null && voice.isPlaying ? .15f : 1f);
            if (ambience.clip != null && !ambience.isPlaying && Time.unscaledTime >= nextAmbientTime)
            {
                ambience.Play();
                nextAmbientTime = Time.unscaledTime + ambience.clip.length + (stageSounds?.ambientGap ?? 12);
            }
        }
        void PlayStageSound(string action)
        {
            var scene = soundCatalog?.stages.FirstOrDefault(s => s.id == currentStage?.id);
            string name = action == "passage" ? scene?.passage : action == "mechanism" ? scene?.mechanism : scene?.discovery;
            Play(Effect(name) ?? found);
        }
        void PlayObjectSound(PropData prop, bool discovered)
        {
            var clip = Effect(prop.sound);
            if (clip != null) Play(clip);
            else if (discovered) PlayStageSound("discovery");
            else Tap();
        }
    }
}
