using System;
using UnityEngine;

namespace BemTeVi
{
    [CreateAssetMenu(menuName = "Bem-Te-Vi/Narração", fileName = "NarrationBank")]
    public sealed class NarrationBank : ScriptableObject
    {
        [Serializable]
        public sealed class Reading
        {
            public string roomId;
            [Range(0, 2)] public int round;
            [TextArea(4, 6)] public string recordingScript;
            [Tooltip("Arraste aqui a sua gravação desta charada. Pode ficar vazio.")]
            public AudioClip clip;
        }
        public Reading[] readings;
        [Serializable]
        public sealed class Discovery
        {
            public string itemId;
            public string recordingScript;
            public AudioClip clip;
        }
        public Discovery[] discoveries;
        public AudioClip ForDiscovery(string itemId)
        {
            if (discoveries != null)
                foreach (var item in discoveries)
                    if (item != null && item.itemId == itemId && item.clip != null) return item.clip;
            return Resources.Load<AudioClip>("BemTeVi/Narration/Found/" + itemId);
        }
        public AudioClip ForRoom(string roomId, int round = 0)
        {
            if (readings != null)
                foreach (var reading in readings)
                    if (reading != null && reading.roomId == roomId && reading.round == round && reading.clip != null) return reading.clip;
            // Optional convention: drop a recording with the room ID as filename here.
            var clip = Resources.Load<AudioClip>("BemTeVi/Narration/" + roomId + "_r" + (round + 1));
            return clip != null || round != 0 ? clip : Resources.Load<AudioClip>("BemTeVi/Narration/" + roomId);
        }
    }
}
