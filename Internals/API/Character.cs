using Character_Alias = Character;

namespace OfTamingAndBreeding.Internals.API
{
    public partial class Character : UnityEngine.MonoBehaviour
    {
        public Character(Character_Alias instance) : base(instance)
        {
        }

    }
}
