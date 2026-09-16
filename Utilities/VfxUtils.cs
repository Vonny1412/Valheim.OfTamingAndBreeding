using System;
using UnityEngine;

namespace OfTamingAndBreeding.Utilities
{
    internal static class VfxUtils
    {

        public static void ScaleVfx(GameObject root, float s)
        {
            if (!root || Mathf.Approximately(s, 1f))
                return;

            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                // Scale emitter position inside the VFX prefab
                if (ps.transform != root.transform)
                {
                    ps.transform.localPosition *= s;
                }

                var main = ps.main;

                main.startSizeMultiplier *= s;
                main.startSpeedMultiplier *= s;
                main.gravityModifierMultiplier *= s;

                var shape = ps.shape;
                if (shape.enabled)
                {
                    // Important: emission offset
                    shape.position *= s;

                    shape.radius *= s;
                    shape.scale *= s;
                }

                var vel = ps.velocityOverLifetime;
                if (vel.enabled)
                {
                    vel.xMultiplier *= s;
                    vel.yMultiplier *= s;
                    vel.zMultiplier *= s;
                }
            }

            foreach (var l in root.GetComponentsInChildren<Light>(true))
            {
                l.range *= s;
            }

            foreach (var tr in root.GetComponentsInChildren<TrailRenderer>(true))
            {
                tr.widthMultiplier *= s;
            }

            foreach (var lr in root.GetComponentsInChildren<LineRenderer>(true))
            {
                lr.widthMultiplier *= s;
            }
        }

    }
}
