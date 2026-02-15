using Character_Alias = Character;
using ItemDrop_Alias = ItemDrop;
using MonsterAI_Alias = MonsterAI;
using Piece_Alias = Piece;
using Player_Alias = Player;
using Tameable_Alias = Tameable;
using ZDOID_Alias = ZDOID;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.ValheimAPI.LowLevel
{
    public partial class Tameable : UnityEngine.MonoBehaviour
    {
        public Tameable(Tameable_Alias instance) : base(instance)
        {
        }

        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(Tameable_Alias), "m_character");

        public static readonly Core.Invokers.FieldMutateInvoker<MonsterAI_Alias> __IAPI_m_monsterAI_Invoker = new Core.Invokers.FieldMutateInvoker<MonsterAI_Alias>(typeof(Tameable_Alias), "m_monsterAI");

        public static readonly Core.Invokers.FieldMutateInvoker<Piece_Alias> __IAPI_m_piece_Invoker = new Core.Invokers.FieldMutateInvoker<Piece_Alias>(typeof(Tameable_Alias), "m_piece");

        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(Tameable_Alias), "m_nview");

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_lastPetTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(Tameable_Alias), "m_lastPetTime");

        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_unsummonTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(Tameable_Alias), "m_unsummonTime");

        public static readonly Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<Player_Alias>> __IAPI_s_nearbyPlayers_Invoker = new Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<Player_Alias>>(typeof(Tameable_Alias), "s_nearbyPlayers");

        public static readonly Core.Invokers.ConstFieldInvoker<float> __IAPI_m_playerMaxDistance_Invoker = new Core.Invokers.ConstFieldInvoker<float>(typeof(Tameable_Alias), "m_playerMaxDistance");

        public static readonly Core.Invokers.ConstFieldInvoker<float> __IAPI_m_tameDeltaTime_Invoker = new Core.Invokers.ConstFieldInvoker<float>(typeof(Tameable_Alias), "m_tameDeltaTime");

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_Tame_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "Tame", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.TypedMethodInvoker<float> __IAPI_GetRemainingTime_Invoker1 = new Core.Invokers.TypedMethodInvoker<float>(typeof(Tameable_Alias), "GetRemainingTime", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_DecreaseRemainingTime_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "DecreaseRemainingTime", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_OnConsumedItem_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "OnConsumedItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ItemDrop_Alias), false) });

        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetName_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "SetName", new Core.Signatures.ParamSig[] { });

        public static readonly Core.Invokers.TypedMethodInvoker<Player_Alias> __IAPI_GetPlayer_Invoker1 = new Core.Invokers.TypedMethodInvoker<Player_Alias>(typeof(Tameable_Alias), "GetPlayer", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ZDOID_Alias), false) });

    }
}
