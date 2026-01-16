
using UnityEngine_Component_Alias = UnityEngine.Component;
using UnityEngine_GameObject_Alias = UnityEngine.GameObject;
using UnityEngine_Transform_Alias = UnityEngine.Transform;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API.UnityEngine
{
    public partial class Component : UnityEngine.Object
    {

        public Component(UnityEngine_Component_Alias instance) : base(instance)
        {
        }

        public UnityEngine_Transform_Alias transform
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).transform;
        }
        public UnityEngine_GameObject_Alias gameObject
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).gameObject;
        }
        public string tag
        {
            get => ((UnityEngine_Component_Alias)__IAPI_instance).tag;
            set => ((UnityEngine_Component_Alias)__IAPI_instance).tag = value;
        }





    }
}
