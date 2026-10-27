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


            public static int s_GetHudTopPoint;
            public static int s_ShowHudPrefix;
            public static int s_ShowHudPostfix;


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
                    ("s_GetHudTopPoint", s_GetHudTopPoint),
                    ("s_ShowHudPrefix", s_ShowHudPrefix),
                    ("s_ShowHudPostfix", s_ShowHudPostfix),
                };

                var fps = s_frames / s_timer;
                Plugin.LogMessage($"[Performance] FPS: {fps:F1}");

                foreach (var entry in entries)
                {
                    var callsPerFrame = entry.Item2 / fps;
                    Plugin.LogMessage($"[Performance] {entry.Item1}: {callsPerFrame:F1}/f ({entry.Item2} calls)");
                }

                s_timer = 0f;
                s_frames = 0;

                s_GetHudTopPoint = 0;
                s_ShowHudPrefix = 0;
                s_ShowHudPostfix = 0;
            }
        }

    }
}