
using Character_Alias = Character;

using OfTamingAndBreeding.Internals.API.Core;
using OfTamingAndBreeding.Internals.API.Core.Invokers;
using OfTamingAndBreeding.Internals.API.Core.Signatures;
namespace OfTamingAndBreeding.Internals.API
{
    public partial class Character : UnityEngine.MonoBehaviour
    {

        public Character(Character_Alias instance) : base(instance)
        {
        }


    }
}
