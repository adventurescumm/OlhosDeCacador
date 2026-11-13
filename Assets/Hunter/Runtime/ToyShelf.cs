using System;
using UnityEngine;

namespace OlhosDeCacador
{
    // The menu displays actual 3D game assets, not generated preview illustrations.
    public sealed class ToyShelf : IDisposable
    {
        readonly RoomView[] worlds = new RoomView[3];
        public ToyShelf(Catalog catalog)
        {
            try
            {
                var preview = new Progress(catalog);
                for (int i = 0; i < worlds.Length; i++)
                {
                    worlds[i] = new RoomView(catalog.maps[i], catalog.maps[i].rooms[0], preview, true);
                    worlds[i].Root.name = "Brinquedo do menu: " + catalog.maps[i].title;
                    worlds[i].Root.transform.localScale = Vector3.one * .43f;
                    worlds[i].Root.transform.position = new Vector3((i - 1) * 5.6f, 0, 0);
                    // Decorative previews do not need physics or target hover updates.
                    foreach (var collider in worlds[i].Root.GetComponentsInChildren<Collider>()) collider.enabled = false;
                    foreach (var target in worlds[i].Root.GetComponentsInChildren<HuntTarget>()) target.enabled = false;
                    foreach (var hotspot in worlds[i].Root.GetComponentsInChildren<WorldHotspot>()) hotspot.enabled = false;
                }
            }
            catch { Dispose(); throw; }
        }
        public void Pose(float time, bool animate)
        {
            for (int i = 0; i < worlds.Length; i++)
            {
                var root = worlds[i]?.Root;
                if (root == null) continue;
                float phase = time * .8f + i * 1.4f;
                root.transform.position = new Vector3((i - 1) * 5.6f, animate ? Mathf.Sin(phase) * .055f : 0, 0);
                root.transform.localRotation = Quaternion.Euler(0, animate ? Mathf.Sin(phase) * 2 : 0, 0);
            }
        }
        public void Dispose() { foreach (var world in worlds) world?.Dispose(); }
    }
}
