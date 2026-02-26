using OfTamingAndBreeding.Data.Models;
using OfTamingAndBreeding.Utils;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace OfTamingAndBreeding.Components
{
    public sealed class OTAB_ConsumeClipOverlay : MonoBehaviour
    {
        [SerializeField] internal string m_animClipName;

        [NonSerialized] private PlayableGraph _graph;
        [NonSerialized] private AnimationClip _animClip;

        private void Awake()
        {
            AnimationUtils.AnimationExists(gameObject, m_animClipName, out _animClip);
        }

        internal void PlayOverlay(Animator animator, float speed = 1f)
        {
            if (!animator) return;
            if (!_animClip) return;

            StopOverlay();

            float speedSafe = Mathf.Max(0.01f, speed);
            float duration = Mathf.Max(0.05f, _animClip.length / speedSafe);

            _graph = PlayableGraph.Create($"OTAB_ConsumeOverlay_{GetInstanceID()}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(_graph, "OTAB_Out", animator);

            var clipPlayable = AnimationClipPlayable.Create(_graph, _animClip);
            clipPlayable.SetSpeed(speedSafe);
            clipPlayable.SetApplyFootIK(false);

            // Start at 0 explicitly
            clipPlayable.SetTime(0);

            var mixer = AnimationMixerPlayable.Create(_graph, 1);
            _graph.Connect(clipPlayable, 0, mixer, 0);
            mixer.SetInputWeight(0, 1f);

            output.SetSourcePlayable(mixer);

            _graph.Play();

            CancelInvoke(nameof(StopOverlay));
            Invoke(nameof(StopOverlay), duration);
        }

        internal void StopOverlay()
        {
            CancelInvoke(nameof(StopOverlay));

            if (_graph.IsValid())
                _graph.Destroy();
        }

        private void OnDisable() => StopOverlay();
        private void OnDestroy() => StopOverlay();
    }
}