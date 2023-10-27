using HarmonyLib;
using RimWorld;
using Verse;

namespace AllForOneGene
{
    public class AllForOneGeneMod : Mod
    {
        public AllForOneGeneMod(ModContentPack pack) : base(pack)
        {
			new Harmony("AllForOneGeneMod").PatchAll();
        }
    }
}
