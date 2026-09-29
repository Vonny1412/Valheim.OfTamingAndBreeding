using UnityEngine;

namespace OfTamingAndBreeding
{
    // currently not used
    public sealed partial class Plugin
    {
        internal static class Performance
        {
            private const float ReportInterval = 10f;

            private static float s_timer;
            private static int s_frames;

            // Counters
            public static int s_isEnemyCalls;
            public static int s_baseAIUpdateCalls;

            public static void Update()
            {
                s_frames++;
                s_timer += Time.unscaledDeltaTime;

                if (s_timer < ReportInterval)
                {
                    return;
                }

                var fps = s_frames / s_timer;

                var entries = new[]
                {
                ("IsEnemy", s_isEnemyCalls),
                ("BaseAI.UpdateAI", s_baseAIUpdateCalls),
            };

                Plugin.LogMessage($"[Performance] FPS: {fps:F1}");

                foreach (var entry in entries)
                {
                    var callsPerSecond = entry.Item2 / s_timer;

                    Plugin.LogMessage($"[Performance] {entry.Item1}: {callsPerSecond:F1}/s ({entry.Item2} calls)");
                }

                // Reset
                s_timer = 0f;
                s_frames = 0;

                s_isEnemyCalls = 0;
                s_baseAIUpdateCalls = 0;
            }
        }

    }
}