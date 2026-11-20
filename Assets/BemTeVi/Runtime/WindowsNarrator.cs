using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BemTeVi
{
    // Windows' installed speech engine; no generated audio files or network service.
    // Keep System.Speech outside Unity/Mono, whose COM support varies by runtime.
    public sealed class WindowsNarrator : IDisposable
    {
        Process process;
        public static bool Supported
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return true;
#else
                return false;
#endif
            }
        }
        public bool IsSpeaking
        {
            get
            {
                if (process == null) return false;
                try
                {
                    if (!process.HasExited) return true;
                    if (process.ExitCode != 0) UnityEngine.Debug.LogWarning("A voz do Windows não pôde ler. Importe uma gravação no NarrationBank ou confira as vozes instaladas.");
                }
                catch (InvalidOperationException) { }
                process.Dispose(); process = null; return false;
            }
        }
        public bool Speak(string text, float volume)
        {
            Stop();
            if (!Supported || string.IsNullOrWhiteSpace(text) || volume <= 0) return false;
            try
            {
                // Only base64 data and a bounded integer enter the fixed script. No shell interpolation.
                string content = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
                string script = "$ErrorActionPreference='Stop'; Add-Type -AssemblyName System.Speech; " +
                    "$v=New-Object System.Speech.Synthesis.SpeechSynthesizer; try { " +
                    "$pt=@($v.GetInstalledVoices() | Where-Object { $_.Enabled -and $_.VoiceInfo.Culture.Name -eq 'pt-BR' }); " +
                    "if($pt.Count -eq 0){$pt=@($v.GetInstalledVoices() | Where-Object { $_.Enabled -and $_.VoiceInfo.Culture.TwoLetterISOLanguageName -eq 'pt' })}; " +
                    "if($pt.Count -gt 0){$v.SelectVoice($pt[0].VoiceInfo.Name)}; " +
                    "$v.Volume=" + Math.Max(0, Math.Min(100, (int)(volume * 100))) + "; $v.Rate=0; " +
                    "$v.Speak([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + content + "'))) " +
                    "} finally { $v.Dispose() }";
                var start = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"),
                    Arguments = "-NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script)),
                    UseShellExecute = false, CreateNoWindow = true
                };
                process = Process.Start(start); return process != null;
            }
            catch (Exception e)
            { UnityEngine.Debug.LogWarning("Voz do Windows indisponível: " + e.Message); Stop(); return false; }
        }
        public void Stop()
        {
            if (process == null) return;
            try { if (!process.HasExited) process.Kill(); }
            catch (Exception e) when (e is InvalidOperationException || e is System.ComponentModel.Win32Exception) { }
            finally { process.Dispose(); process = null; }
        }
        public void Dispose() => Stop();
    }
}
