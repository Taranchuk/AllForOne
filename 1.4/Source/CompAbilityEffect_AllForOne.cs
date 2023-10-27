using Multiplayer.API;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace AllForOneGene
{
    public class CompProperties_AllForOne : CompProperties_AbilityEffect
    {
        public CompProperties_AllForOne()
        {
            compClass = typeof(CompAbilityEffect_AllForOne);
        }
    }

    public class CompAbilityEffect_AllForOne : CompAbilityEffect
    {
        public new CompProperties_AllForOne Props => (CompProperties_AllForOne)props;
        public List<Gene> casterGenesToGive = new List<Gene>();
        public List<Gene> targetGenesToTake = new List<Gene>();

        [SyncMethod] public void AddToCasterGenesToGiveList(Gene gene) 
        {
            if (casterGenesToGive.Contains(gene) is false)
            {
                casterGenesToGive.Add(gene);
            }
        }
        [SyncMethod] public void RemoveFromCasterGenesToGiveList(Gene gene) => casterGenesToGive.Remove(gene);

        [SyncMethod] public void AddToTargetGenesToTakeList(Gene gene)
        {
            if (targetGenesToTake.Contains(gene) is false)
            {
                targetGenesToTake.Add(gene);
            }
        }
        [SyncMethod] public void RemoveFromTargetGenesToTakeList(Gene gene) => targetGenesToTake.Remove(gene);

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Pawn is null || target.Pawn.RaceProps.Humanlike is false)
            {
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            try
            {
                var comp = parent.pawn.CurJob.ability.CompOfType<CompAbilityEffect_AllForOne>();
                //if (comp.casterGenesToGive.Any())
                //{
                //    target.Pawn.health.AddHediff(AllForOne_DefOf.ForcefulGeneImplantationComa);
                //}
                //if (comp.targetGenesToTake.Any())
                //{
                //    target.Pawn.health.AddHediff(AllForOne_DefOf.ForcefulGeneRemovalComa);
                //}
                foreach (var gene in casterGenesToGive.ToList())
                {
                    target.Pawn.genes.AddGene(gene.def, true);
                    var pawnGene = parent.pawn.genes.GetGene(gene.def);
                    if (pawnGene != null)
                    {
                        parent.pawn.genes.RemoveGene(pawnGene);
                    }
                }
                foreach (var gene in targetGenesToTake.ToList())
                {
                    var targetGene = target.Pawn.genes.GetGene(gene.def);
                    if (targetGene != null)
                    {
                        target.Pawn.genes.RemoveGene(targetGene);
                    }
                    parent.pawn.genes.AddGene(gene.def, true);
                }
                ClearLists();
            }
            catch (Exception ex)
            {
                Log.Error("Error applying ability: " + ex.ToString() + " - " + new StackTrace());
            }
        }

        [SyncMethod]
        public void ClearLists()
        {
            casterGenesToGive.Clear();
            targetGenesToTake.Clear();
        }

        public static void ThrowRedSmoke(Vector3 loc, Map map, float size)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, size);
            dataStatic.rotationRate = 0;
            dataStatic.velocityAngle = 200;
            dataStatic.velocitySpeed = 0;
            dataStatic.instanceColor = Color.red;
            map.flecks.CreateFleck(dataStatic);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref casterGenesToGive, "casterGenesToGive", LookMode.Reference);
            Scribe_Collections.Look(ref targetGenesToTake, "targetGenesToTake", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                casterGenesToGive ??= new List<Gene>();
                targetGenesToTake ??= new List<Gene>();
            }
        }
    }
}
