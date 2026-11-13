using System;
using UnityEngine;

namespace OlhosDeCacador
{
    [CreateAssetMenu(menuName = "Olhos de Caçador/Narração", fileName = "NarrationBank")]
    public sealed class NarrationBank : ScriptableObject
    {
        [Serializable]
        public sealed class Reading
        {
            public string roomId;
            [TextArea(4, 6)] public string recordingScript;
            [Tooltip("Arraste aqui a sua gravação desta charada. Pode ficar vazio.")]
            public AudioClip clip;
        }
        public Reading[] readings;
        public AudioClip ForRoom(string roomId)
        {
            if (readings != null)
                foreach (var reading in readings)
                    if (reading != null && reading.roomId == roomId && reading.clip != null) return reading.clip;
            // Optional convention: drop a recording with the room ID as filename here.
            return Resources.Load<AudioClip>("Hunter/Narration/" + roomId);
        }
    }
}
