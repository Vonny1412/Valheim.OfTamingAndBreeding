using OfTamingAndBreeding.Components.Base;
using OfTamingAndBreeding.OTABUtils;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace OfTamingAndBreeding.Components
{
    public class AnimationClipOverlay : OTABComponent<AnimationClipOverlay>
    {
        [SerializeField] public string m_animClipName;

        // Bridge-Tuning
        [SerializeField] public float m_endPoseHoldSeconds = 0.08f;
        [SerializeField] public float m_endPoseFadeSeconds = 0.12f;

        // how close to the end we “freeze”
        [SerializeField] public float m_endPoseEpsilonSeconds = 0.02f;

        private PlayableGraph _graph;
        private AnimationClip _animClip;

        private AnimationLayerMixerPlayable _layerMixer;
        private AnimatorControllerPlayable _baseControllerPlayable;

        private AnimationClipPlayable _overlayPlayable; // running
        private AnimationClipPlayable _endPosePlayable; // freeze-frame

        private Animator _animator;
        private bool _isPlaying;

        // state
        private bool _endPosePrepared;
        private bool _bridgeActive;
        private float _holdLeft;
        private float _fadeLeft;

        // robust timing (not dependent on GetDuration)
        private float _startTime;
        private float _duration;      // seconds (clip.length / speed)
        private float _freezeAt;      // seconds into clip

        private void Awake()
        {
            AnimationUtils.AnimationExists(gameObject, m_animClipName, out _animClip);
        }

        public void PlayOverlay(Animator animator, float speed = 1f)
        {
            if (!animator) return;
            if (!_animClip) return;
            if (!animator.runtimeAnimatorController) return;

            StopOverlay();

            _animator = animator;

            float speedSafe = Mathf.Max(0.01f, speed);
            _duration = Mathf.Max(0.05f, _animClip.length / speedSafe);
            _startTime = Time.time;

            // freeze point near end
            _freezeAt = Mathf.Clamp(_animClip.length - Mathf.Max(0.005f, m_endPoseEpsilonSeconds), 0f, _animClip.length);

            _graph = PlayableGraph.Create($"OTAB_ConsumeOverlay_{GetInstanceID()}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(_graph, "OTAB_Consume_Out", animator);

            _baseControllerPlayable = AnimatorControllerPlayable.Create(_graph, animator.runtimeAnimatorController);

            _overlayPlayable = AnimationClipPlayable.Create(_graph, _animClip);
            _overlayPlayable.SetSpeed(speedSafe);
            _overlayPlayable.SetApplyFootIK(false);
            _overlayPlayable.SetTime(0);

            _endPosePlayable = AnimationClipPlayable.Create(_graph, _animClip);
            _endPosePlayable.SetSpeed(0f);
            _endPosePlayable.SetApplyFootIK(false);
            _endPosePlayable.SetTime(_freezeAt);

            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 3);

            _graph.Connect(_baseControllerPlayable, 0, _layerMixer, 0);
            _layerMixer.SetInputWeight(0, 1f);

            _graph.Connect(_overlayPlayable, 0, _layerMixer, 1);
            _layerMixer.SetInputWeight(1, 1f);

            _graph.Connect(_endPosePlayable, 0, _layerMixer, 2);
            _layerMixer.SetInputWeight(2, 0f);

            output.SetSourcePlayable(_layerMixer);

            _endPosePrepared = false;
            _bridgeActive = false;
            _holdLeft = 0f;
            _fadeLeft = 0f;

            _graph.Play();
            _isPlaying = true;
        }

        private void Update()
        {
            if (!_isPlaying) return;
            if (!_graph.IsValid()) return;

            if (_bridgeActive)
            {
                TickBridge();
                return;
            }

            float elapsed = Time.time - _startTime;

            // Shortly before the end: prepare the final pose once
            if (!_endPosePrepared && elapsed >= (_duration - Mathf.Max(0.01f, m_endPoseEpsilonSeconds)))
            {
                // we explicitly set the endpose playable time again, 
                // in case Unity/Clip settings move anything
                _endPosePlayable.SetTime(_freezeAt);
                _endPosePlayable.SetSpeed(0f);
                _endPosePrepared = true;
            }

            // fertig? -> Bridge
            if (elapsed >= _duration)
            {
                BeginBridge();
            }
        }

        private void BeginBridge()
        {
            _bridgeActive = true;

            if (_layerMixer.IsValid())
            {
                _layerMixer.SetInputWeight(1, 0f); // overlay off
                _layerMixer.SetInputWeight(2, 1f); // freeze endpose on
            }

            _holdLeft = Mathf.Max(0f, m_endPoseHoldSeconds);
            _fadeLeft = Mathf.Max(0.02f, m_endPoseFadeSeconds);
        }

        private void TickBridge()
        {
            if (_holdLeft > 0f)
            {
                _holdLeft -= Time.deltaTime;
                if (_holdLeft > 0f) return;
            }

            _fadeLeft -= Time.deltaTime;
            float w = (_fadeLeft <= 0f) ? 0f : Mathf.Clamp01(_fadeLeft / Mathf.Max(0.0001f, m_endPoseFadeSeconds));

            if (_layerMixer.IsValid())
                _layerMixer.SetInputWeight(2, w);

            if (w <= 0.0001f)
                DestroyGraph();
        }

        private void DestroyGraph()
        {
            if (_graph.IsValid())
                _graph.Destroy();

            if (_animator)
                _animator.Update(0f);

            _isPlaying = false;
            _endPosePrepared = false;
            _bridgeActive = false;
            _holdLeft = 0f;
            _fadeLeft = 0f;
            _duration = 0f;
            _freezeAt = 0f;
        }

        internal void StopOverlay()
        {
            if (_graph.IsValid())
                _graph.Destroy();

            _isPlaying = false;
            _endPosePrepared = false;
            _bridgeActive = false;
            _holdLeft = 0f;
            _fadeLeft = 0f;
            _duration = 0f;
            _freezeAt = 0f;
        }

        private void OnDisable() => StopOverlay();
        private void OnDestroy() => StopOverlay();

        public bool IsPlaying() => _isPlaying;
    }
}