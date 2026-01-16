
using UnityEngine_Behaviour_Alias = UnityEngine.Behaviour;

using System;
using System.ComponentModel;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API.UnityEngine
{
    public partial class Behaviour : UnityEngine.Component
    {
        public static new System.Type __IAPI_GetInstanceType() => typeof(UnityEngine_Behaviour_Alias);
        public Behaviour(UnityEngine_Behaviour_Alias instance) : base(instance)
        {
        }
        public new UnityEngine_Behaviour_Alias __IAPI_GetInstance() => (UnityEngine_Behaviour_Alias)__IAPI_instance;
        public void __IAPI_SetInstance(UnityEngine_Behaviour_Alias instance) => __IAPI_instance = instance;
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
