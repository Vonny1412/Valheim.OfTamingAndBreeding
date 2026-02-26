using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OfTamingAndBreeding.Components
{
    public sealed class OTAB_Egg : MonoBehaviour
    {

        //
        // EggGrow
        //

        [SerializeField] internal Heightmap.Biome m_requireBiome = Heightmap.Biome.None;
        [SerializeField] internal Utils.EnvironmentUtils.LiquidTypeEx m_requireLiquid = Utils.EnvironmentUtils.LiquidTypeEx.None;
        [SerializeField] internal float m_requireLiquidDepth = 0;
        [SerializeField] private int m_grownListIndex = -1;

        internal void SetCustomGrownList(Data.Models.EggData.EggGrowGrownData[] grownList)
        {
            m_grownListIndex = StaticContext.EggDataContext.grownListByIndex.Count;
            StaticContext.EggDataContext.grownListByIndex.Add(grownList);
        }

        internal bool HasCustomGrownList(out Data.Models.EggData.EggGrowGrownData[] grownList)
        {
            if (m_grownListIndex != -1)
            {
                grownList = StaticContext.EggDataContext.grownListByIndex[m_grownListIndex];
                return true;
            }
            grownList = null;
            return false;
        }






    }

}
