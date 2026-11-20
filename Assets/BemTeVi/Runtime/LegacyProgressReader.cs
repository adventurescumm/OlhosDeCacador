using System;
using System.Collections.Generic;
using System.Text;

namespace BemTeVi
{
    internal static class LegacyProgressReader
    {
        // Read-only migration identifiers, deliberately retained after the product rename.
        public static IEnumerable<string> ReadWindowsSaves()
        {
            var values = new List<string>();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            foreach (string company in new[] { "OlhosDeCacador", "BemTeVi" })
            foreach (string prefix in new[] { @"Software\", @"Software\Unity\UnityEditor\" })
            {
                try
                {
                    var registry = Type.GetType("Microsoft.Win32.Registry, mscorlib") ?? Type.GetType("Microsoft.Win32.Registry, Microsoft.Win32.Registry");
                    var user = registry?.GetField("CurrentUser")?.GetValue(null);
                    if (user == null) continue;
                    var key = user.GetType().GetMethod("OpenSubKey", new[] { typeof(string), typeof(bool) })
                        ?.Invoke(user, new object[] { prefix + company + @"\Olhos de Caçador", false });
                    using (key as IDisposable)
                    {
                        if (key == null) continue;
                        foreach (string wanted in new[] { "BemTeVi.v1", "BemTeVi.v1.backup", "OlhosDeCacador.v1", "OlhosDeCacador.v1.backup" })
                        foreach (string name in (string[])key.GetType().GetMethod("GetValueNames").Invoke(key, null))
                        {
                            if (name != wanted && !name.StartsWith(wanted + "_h", StringComparison.Ordinal)) continue;
                            var value = key.GetType().GetMethod("GetValue", new[] { typeof(string) }).Invoke(key, new object[] { name });
                            string text = value is byte[] bytes ? Encoding.UTF8.GetString(bytes).TrimEnd('\0') : value as string;
                            if (!string.IsNullOrEmpty(text)) values.Add(text);
                        }
                    }
                }
                catch (Exception e) { UnityEngine.Debug.LogWarning("Progresso antigo não acessível: " + e.Message); }
            }
#endif
            return values;
        }
    }
}
