
using UnityEngine_Object_Alias = UnityEngine.Object;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API.UnityEngine
{
    public partial class Object : Core.ClassPublicizer
    {

        public Object(UnityEngine_Object_Alias instance) : base(instance)
        {
        }


        public string name
        {
            get => ((UnityEngine_Object_Alias)__IAPI_instance).name;
            set => ((UnityEngine_Object_Alias)__IAPI_instance).name = value;
        }


    }
}
