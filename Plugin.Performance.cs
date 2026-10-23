using UnityEngine;

namespace OfTamingAndBreeding
{
    public sealed partial class Plugin
    {
        internal static class Performance
        {
            private const float ReportInterval = 10f;

            private static float s_timer;
            private static int s_frames;

            
            public static int s_updateHostilities;
            

            public static void Update()
            {
                s_frames++;
                s_timer += Time.unscaledDeltaTime;

                if (s_timer < ReportInterval)
                {
                    return;
                }

                var entries = new[]
                {
                    ("CharacterTrait.UpdateHostilities", s_updateHostilities),
                };

                var fps = s_frames / s_timer;
                Plugin.LogMessage($"[Performance] FPS: {fps:F1}");

                foreach (var entry in entries)
                {
                    var callsPerSecond = entry.Item2 / s_timer;
                    Plugin.LogMessage($"[Performance] {entry.Item1}: {callsPerSecond:F1}/s ({entry.Item2} calls)");
                }

                s_timer = 0f;
                s_frames = 0;

                s_updateHostilities = 0;
            }
        }

    }
}