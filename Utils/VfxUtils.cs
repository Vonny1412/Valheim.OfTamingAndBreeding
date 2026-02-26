using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Utils
{
    internal static class VfxUtils
    {

        public static void ScaleVfx(GameObject root, float s)
        {
            if (!root || Mathf.Approximately(s, 1f)) return;

            // 1) ParticleSystems
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;

                // Ensure they follow hierarchy scale when possible
                // (doesn't exist in all Unity versions, so keep it defensive)
                // main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                // Multiply common size/speed values
                main.startSizeMultiplier *= s;
                main.startSpeedMultiplier *= s;
                main.startRotationMultiplier *= 1f; // usually don't scale rotation
                main.gravityModifierMultiplier *= s;

                // Shape sizes
                var shape = ps.shape;
                if (shape.enabled)
                {
                    shape.radius *= s;
                    shape.radiusThickness *= 1f; // keep thickness as-is
                    shape.scale *= s;
                }

                // Velocity over lifetime (world-space feel)
                var vel = ps.velocityOverLifetime;
                if (vel.enabled)
                {
                    vel.xMultiplier *= s;
                    vel.yMultiplier *= s;
                    vel.zMultiplier *= s;
                }
            }

            // 2) Lights
            foreach (var l in root.GetComponentsInChildren<Light>(true))
            {
                l.range *= s;
                // intensity usually NOT scaled linearly; optional:
                // l.intensity *= Mathf.Sqrt(s);
            }

            // 3) TrailRenderers / LineRenderers
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
