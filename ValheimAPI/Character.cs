using Character_Alias = Character;
using UnityEngine_Rigidbody_Alias = UnityEngine.Rigidbody;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class Character : UnityEngine.MonoBehaviour
    {
        public Character(Character_Alias instance) : base(instance)
        {
        }

        //public static readonly Core.Invokers.FieldMutateInvoker<UnityEngine_Rigidbody_Alias> __IAPI_m_body_Invoker = new Core.Invokers.FieldMutateInvoker<UnityEngine_Rigidbody_Alias>(typeof(Character_Alias), "m_body");

    }
}
