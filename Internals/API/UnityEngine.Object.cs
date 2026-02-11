using UnityEngine_Object_Alias = UnityEngine.Object;

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
