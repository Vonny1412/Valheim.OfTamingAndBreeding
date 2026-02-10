using Character_Alias = Character;
using EffectList_Alias = EffectList;
using Humanoid_Alias = Humanoid;
using ItemDrop_Alias = ItemDrop;
using MonsterAI_Alias = MonsterAI;
using Piece_Alias = Piece;
using Player_Alias = Player;
using Sadle_Alias = Sadle;
using Skills_SkillType_Alias = Skills.SkillType;
using Tameable_Alias = Tameable;
using Tameable_TextGetter_Alias = Tameable.TextGetter;
using UnityEngine_Vector3_Alias = UnityEngine.Vector3;
using ZDOID_Alias = ZDOID;
using ZNetView_Alias = ZNetView;

namespace OfTamingAndBreeding.Internals.API
{
    public partial class Tameable : UnityEngine.MonoBehaviour
    {
        public Tameable(Tameable_Alias instance) : base(instance)
        {
        }




        public float m_fedDuration
        {
            get => ((Tameable_Alias)__IAPI_instance).m_fedDuration;
            set => ((Tameable_Alias)__IAPI_instance).m_fedDuration = value;
        }
        public float m_tamingTime
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tamingTime;
            set => ((Tameable_Alias)__IAPI_instance).m_tamingTime = value;
        }
        public bool m_startsTamed
        {
            get => ((Tameable_Alias)__IAPI_instance).m_startsTamed;
            set => ((Tameable_Alias)__IAPI_instance).m_startsTamed = value;
        }
        public EffectList_Alias m_tamedEffect
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tamedEffect;
            set => ((Tameable_Alias)__IAPI_instance).m_tamedEffect = value;
        }
        public EffectList_Alias m_sootheEffect
        {
            get => ((Tameable_Alias)__IAPI_instance).m_sootheEffect;
            set => ((Tameable_Alias)__IAPI_instance).m_sootheEffect = value;
        }
        public EffectList_Alias m_petEffect
        {
            get => ((Tameable_Alias)__IAPI_instance).m_petEffect;
            set => ((Tameable_Alias)__IAPI_instance).m_petEffect = value;
        }
        public bool m_commandable
        {
            get => ((Tameable_Alias)__IAPI_instance).m_commandable;
            set => ((Tameable_Alias)__IAPI_instance).m_commandable = value;
        }
        public float m_unsummonDistance
        {
            get => ((Tameable_Alias)__IAPI_instance).m_unsummonDistance;
            set => ((Tameable_Alias)__IAPI_instance).m_unsummonDistance = value;
        }
        public float m_unsummonOnOwnerLogoutSeconds
        {
            get => ((Tameable_Alias)__IAPI_instance).m_unsummonOnOwnerLogoutSeconds;
            set => ((Tameable_Alias)__IAPI_instance).m_unsummonOnOwnerLogoutSeconds = value;
        }
        public EffectList_Alias m_unSummonEffect
        {
            get => ((Tameable_Alias)__IAPI_instance).m_unSummonEffect;
            set => ((Tameable_Alias)__IAPI_instance).m_unSummonEffect = value;
        }
        public Skills_SkillType_Alias m_levelUpOwnerSkill
        {
            get => ((Tameable_Alias)__IAPI_instance).m_levelUpOwnerSkill;
            set => ((Tameable_Alias)__IAPI_instance).m_levelUpOwnerSkill = value;
        }
        public float m_levelUpFactor
        {
            get => ((Tameable_Alias)__IAPI_instance).m_levelUpFactor;
            set => ((Tameable_Alias)__IAPI_instance).m_levelUpFactor = value;
        }
        public ItemDrop_Alias m_saddleItem
        {
            get => ((Tameable_Alias)__IAPI_instance).m_saddleItem;
            set => ((Tameable_Alias)__IAPI_instance).m_saddleItem = value;
        }
        public Sadle_Alias m_saddle
        {
            get => ((Tameable_Alias)__IAPI_instance).m_saddle;
            set => ((Tameable_Alias)__IAPI_instance).m_saddle = value;
        }
        public bool m_dropSaddleOnDeath
        {
            get => ((Tameable_Alias)__IAPI_instance).m_dropSaddleOnDeath;
            set => ((Tameable_Alias)__IAPI_instance).m_dropSaddleOnDeath = value;
        }
        public UnityEngine_Vector3_Alias m_dropSaddleOffset
        {
            get => ((Tameable_Alias)__IAPI_instance).m_dropSaddleOffset;
            set => ((Tameable_Alias)__IAPI_instance).m_dropSaddleOffset = value;
        }
        public float m_dropItemVel
        {
            get => ((Tameable_Alias)__IAPI_instance).m_dropItemVel;
            set => ((Tameable_Alias)__IAPI_instance).m_dropItemVel = value;
        }
        public System.Collections.Generic.List<string> m_randomStartingName
        {
            get => ((Tameable_Alias)__IAPI_instance).m_randomStartingName;
            set => ((Tameable_Alias)__IAPI_instance).m_randomStartingName = value;
        }
        public float m_tamingSpeedMultiplierRange
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tamingSpeedMultiplierRange;
            set => ((Tameable_Alias)__IAPI_instance).m_tamingSpeedMultiplierRange = value;
        }
        public float m_tamingBoostMultiplier
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tamingBoostMultiplier;
            set => ((Tameable_Alias)__IAPI_instance).m_tamingBoostMultiplier = value;
        }
        public bool m_nameBeforeText
        {
            get => ((Tameable_Alias)__IAPI_instance).m_nameBeforeText;
            set => ((Tameable_Alias)__IAPI_instance).m_nameBeforeText = value;
        }
        public string m_tameText
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tameText;
            set => ((Tameable_Alias)__IAPI_instance).m_tameText = value;
        }
        public Tameable_TextGetter_Alias m_tameTextGetter
        {
            get => ((Tameable_Alias)__IAPI_instance).m_tameTextGetter;
            set => ((Tameable_Alias)__IAPI_instance).m_tameTextGetter = value;
        }
        public static readonly Core.Invokers.FieldMutateInvoker<Character_Alias> __IAPI_m_character_Invoker = new Core.Invokers.FieldMutateInvoker<Character_Alias>(typeof(Tameable_Alias), "m_character");
        public Character_Alias m_character
        {
            get => __IAPI_m_character_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_character_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<MonsterAI_Alias> __IAPI_m_monsterAI_Invoker = new Core.Invokers.FieldMutateInvoker<MonsterAI_Alias>(typeof(Tameable_Alias), "m_monsterAI");
        public MonsterAI_Alias m_monsterAI
        {
            get => __IAPI_m_monsterAI_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_monsterAI_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<Piece_Alias> __IAPI_m_piece_Invoker = new Core.Invokers.FieldMutateInvoker<Piece_Alias>(typeof(Tameable_Alias), "m_piece");
        public Piece_Alias m_piece
        {
            get => __IAPI_m_piece_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_piece_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<ZNetView_Alias> __IAPI_m_nview_Invoker = new Core.Invokers.FieldMutateInvoker<ZNetView_Alias>(typeof(Tameable_Alias), "m_nview");
        public ZNetView_Alias m_nview
        {
            get => __IAPI_m_nview_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_nview_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_lastPetTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(Tameable_Alias), "m_lastPetTime");
        public float m_lastPetTime
        {
            get => __IAPI_m_lastPetTime_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_lastPetTime_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<float> __IAPI_m_unsummonTime_Invoker = new Core.Invokers.FieldMutateInvoker<float>(typeof(Tameable_Alias), "m_unsummonTime");
        public float m_unsummonTime
        {
            get => __IAPI_m_unsummonTime_Invoker.Get(((Tameable_Alias)__IAPI_instance));
            set => __IAPI_m_unsummonTime_Invoker.Set(((Tameable_Alias)__IAPI_instance), value);
        }
        public static readonly Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<Player_Alias>> __IAPI_s_nearbyPlayers_Invoker = new Core.Invokers.FieldMutateInvoker<System.Collections.Generic.List<Player_Alias>>(typeof(Tameable_Alias), "s_nearbyPlayers");
        public static System.Collections.Generic.List<Player_Alias> s_nearbyPlayers
        {
            get => __IAPI_s_nearbyPlayers_Invoker.Get(null);
            set => __IAPI_s_nearbyPlayers_Invoker.Set(null, value);
        }
        public static readonly Core.Invokers.ConstFieldInvoker<float> __IAPI_m_playerMaxDistance_Invoker = new Core.Invokers.ConstFieldInvoker<float>(typeof(Tameable_Alias), "m_playerMaxDistance");
        public static float m_playerMaxDistance
        {
            get => __IAPI_m_playerMaxDistance_Invoker.Get(null);
        }
        public static readonly Core.Invokers.ConstFieldInvoker<float> __IAPI_m_tameDeltaTime_Invoker = new Core.Invokers.ConstFieldInvoker<float>(typeof(Tameable_Alias), "m_tameDeltaTime");
        public static float m_tameDeltaTime
        {
            get => __IAPI_m_tameDeltaTime_Invoker.Get(null);
        }




        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_Tame_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "Tame", new Core.Signatures.ParamSig[] { });
        public void Tame() => __IAPI_Tame_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { });



        public bool IsTamed() => ((Tameable_Alias)__IAPI_instance).IsTamed();
        public bool IsHungry() => ((Tameable_Alias)__IAPI_instance).IsHungry();






        public static readonly Core.Invokers.TypedMethodInvoker<float> __IAPI_GetRemainingTime_Invoker1 = new Core.Invokers.TypedMethodInvoker<float>(typeof(Tameable_Alias), "GetRemainingTime", new Core.Signatures.ParamSig[] { });
        public float GetRemainingTime() => __IAPI_GetRemainingTime_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { });



        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_DecreaseRemainingTime_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "DecreaseRemainingTime", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(float), false) });
        public void DecreaseRemainingTime(float time) => __IAPI_DecreaseRemainingTime_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { time });




        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_OnConsumedItem_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "OnConsumedItem", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ItemDrop_Alias), false) });
        public void OnConsumedItem(ItemDrop_Alias item) => __IAPI_OnConsumedItem_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { item });


        public bool HaveRider() => ((Tameable_Alias)__IAPI_instance).HaveRider();
        public float GetRiderSkill() => ((Tameable_Alias)__IAPI_instance).GetRiderSkill();



        public static readonly Core.Invokers.VoidMethodInvoker __IAPI_SetName_Invoker1 = new Core.Invokers.VoidMethodInvoker(typeof(Tameable_Alias), "SetName", new Core.Signatures.ParamSig[] { });
        public void SetName() => __IAPI_SetName_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { });



        public static void TameAllInArea(UnityEngine_Vector3_Alias point, float radius) => Tameable_Alias.TameAllInArea(point, radius);
        public void Command(Humanoid_Alias user, bool message = true) => ((Tameable_Alias)__IAPI_instance).Command(user, message);


        public static readonly Core.Invokers.TypedMethodInvoker<Player_Alias> __IAPI_GetPlayer_Invoker1 = new Core.Invokers.TypedMethodInvoker<Player_Alias>(typeof(Tameable_Alias), "GetPlayer", new Core.Signatures.ParamSig[] { new Core.Signatures.NonGenericParamSig(typeof(ZDOID_Alias), false) });
        public Player_Alias GetPlayer(ZDOID_Alias characterID) => __IAPI_GetPlayer_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { characterID });




        public string GetHoverName() => ((Tameable_Alias)__IAPI_instance).GetHoverName();



        /*


        public void Update() => ((Tameable_Alias)__IAPI_instance).Update();
        public string GetHoverText() => ((Tameable_Alias)__IAPI_instance).GetHoverText();
        public string GetStatusString() => ((Tameable_Alias)__IAPI_instance).GetStatusString();

        public string GetName() => ((Tameable_Alias)__IAPI_instance).GetName();
        public virtual bool Interact(Humanoid_Alias user, bool hold, bool alt) => ((Tameable_Alias)__IAPI_instance).Interact(user, hold, alt);
        public string GetHoverName() => ((Tameable_Alias)__IAPI_instance).GetHoverName();

        public virtual string GetText() => ((Tameable_Alias)__IAPI_instance).GetText();
        public virtual void SetText(string text) => ((Tameable_Alias)__IAPI_instance).SetText(text);
        public static readonly VoidMethodInvoker __IAPI_RPC_SetName_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "RPC_SetName", new ParamSig[] {new NonGenericParamSig(typeof(System.Int64), false), new NonGenericParamSig(typeof(string), false), new NonGenericParamSig(typeof(string), false)});
        public void RPC_SetName(System.Int64 sender, string name, string authorId) => __IAPI_RPC_SetName_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { sender, name, authorId });
        public virtual bool UseItem(Humanoid_Alias user, ItemDrop_ItemData_Alias item) => ((Tameable_Alias)__IAPI_instance).UseItem(user, item);
        public static readonly VoidMethodInvoker __IAPI_RPC_AddSaddle_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "RPC_AddSaddle", new ParamSig[] {new NonGenericParamSig(typeof(System.Int64), false)});
        public void RPC_AddSaddle(System.Int64 sender) => __IAPI_RPC_AddSaddle_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { sender });
        public bool DropSaddle(UnityEngine_Vector3_Alias userPoint) => ((Tameable_Alias)__IAPI_instance).DropSaddle(userPoint);
        public static readonly VoidMethodInvoker __IAPI_SpawnSaddle_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "SpawnSaddle", new ParamSig[] {new NonGenericParamSig(typeof(UnityEngine_Vector3_Alias), false)});
        public void SpawnSaddle(UnityEngine_Vector3_Alias flyDirection) => __IAPI_SpawnSaddle_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { flyDirection });
        public static readonly TypedMethodInvoker<bool> __IAPI_HaveSaddle_Invoker1 = new TypedMethodInvoker<bool>(typeof(Tameable_Alias), "HaveSaddle", new ParamSig[] {});
        public bool HaveSaddle() => __IAPI_HaveSaddle_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });
        public static readonly VoidMethodInvoker __IAPI_RPC_SetSaddle_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "RPC_SetSaddle", new ParamSig[] {new NonGenericParamSig(typeof(System.Int64), false), new NonGenericParamSig(typeof(bool), false)});
        public void RPC_SetSaddle(System.Int64 sender, bool enabled) => __IAPI_RPC_SetSaddle_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { sender, enabled });
        public static readonly VoidMethodInvoker __IAPI_SetSaddle_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "SetSaddle", new ParamSig[] {new NonGenericParamSig(typeof(bool), false)});
        public void SetSaddle(bool enabled) => __IAPI_SetSaddle_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { enabled });
        public static readonly VoidMethodInvoker __IAPI_TamingUpdate_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "TamingUpdate", new ParamSig[] {});
        public void TamingUpdate() => __IAPI_TamingUpdate_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });


        public static readonly TypedMethodInvoker<Player_Alias> __IAPI_GetPlayer_Invoker1 = new TypedMethodInvoker<Player_Alias>(typeof(Tameable_Alias), "GetPlayer", new ParamSig[] {new NonGenericParamSig(typeof(ZDOID_Alias), false)});
        public Player_Alias GetPlayer(ZDOID_Alias characterID) => __IAPI_GetPlayer_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { characterID });
        public static readonly VoidMethodInvoker __IAPI_RPC_Command_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "RPC_Command", new ParamSig[] {new NonGenericParamSig(typeof(System.Int64), false), new NonGenericParamSig(typeof(ZDOID_Alias), false), new NonGenericParamSig(typeof(bool), false)});
        public void RPC_Command(System.Int64 sender, ZDOID_Alias characterID, bool message) => __IAPI_RPC_Command_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { sender, characterID, message });
        public static readonly VoidMethodInvoker __IAPI_UpdateSavedFollowTarget_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "UpdateSavedFollowTarget", new ParamSig[] {});
        public void UpdateSavedFollowTarget() => __IAPI_UpdateSavedFollowTarget_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });

        public static readonly VoidMethodInvoker __IAPI_ResetFeedingTimer_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "ResetFeedingTimer", new ParamSig[] {});
        public void ResetFeedingTimer() => __IAPI_ResetFeedingTimer_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });
        public static readonly VoidMethodInvoker __IAPI_OnDeath_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "OnDeath", new ParamSig[] {});
        public void OnDeath() => __IAPI_OnDeath_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });
        public static readonly TypedMethodInvoker<int> __IAPI_GetTameness_Invoker1 = new TypedMethodInvoker<int>(typeof(Tameable_Alias), "GetTameness", new ParamSig[] {});
        public int GetTameness() => __IAPI_GetTameness_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });


        public static readonly VoidMethodInvoker __IAPI_UpdateSummon_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "UpdateSummon", new ParamSig[] {});
        public void UpdateSummon() => __IAPI_UpdateSummon_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });
        public static readonly VoidMethodInvoker __IAPI_UnsummonMaxInstances_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "UnsummonMaxInstances", new ParamSig[] {new NonGenericParamSig(typeof(int), false)});
        public void UnsummonMaxInstances(int maxInstances) => __IAPI_UnsummonMaxInstances_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { maxInstances });
        public static readonly VoidMethodInvoker __IAPI_UnSummon_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "UnSummon", new ParamSig[] {});
        public void UnSummon() => __IAPI_UnSummon_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] {  });
        public static readonly VoidMethodInvoker __IAPI_RPC_UnSummon_Invoker1 = new VoidMethodInvoker(typeof(Tameable_Alias), "RPC_UnSummon", new ParamSig[] {new NonGenericParamSig(typeof(System.Int64), false)});
        public void RPC_UnSummon(System.Int64 sender) => __IAPI_RPC_UnSummon_Invoker1.Invoke(((Tameable_Alias)__IAPI_instance), new object[] { sender });
        
        */
    }
}
