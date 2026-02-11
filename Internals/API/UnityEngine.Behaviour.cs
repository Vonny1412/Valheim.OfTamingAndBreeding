using UnityEngine_Behaviour_Alias = UnityEngine.Behaviour;

namespace OfTamingAndBreeding.Internals.API.UnityEngine
{
    public partial class Behaviour : UnityEngine.Component
    {
        public Behaviour(UnityEngine_Behaviour_Alias instance) : base(instance)
        {
        }

        public bool enabled
        {
            get => ((UnityEngine_Behaviour_Alias)__IAPI_instance).enabled;
            set => ((UnityEngine_Behaviour_Alias)__IAPI_instance).enabled = value;
        }
        public bool isActiveAndEnabled
        {
            get => ((UnityEngine_Behaviour_Alias)__IAPI_instance).isActiveAndEnabled;
        }
        
    }
}
