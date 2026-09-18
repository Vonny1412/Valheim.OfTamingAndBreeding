using EnemyHud_Alias = EnemyHud;

namespace OfTamingAndBreeding.ValheimAPI
{
    public partial class EnemyHud : UnityEngine.MonoBehaviour
    {
        public EnemyHud(EnemyHud_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<object> __IAPI_m_huds_Invoker = new Core.Invokers.FieldMutateInvoker<object>(typeof(EnemyHud_Alias), "m_huds");

    }
}
