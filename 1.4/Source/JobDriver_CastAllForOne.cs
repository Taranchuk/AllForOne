using Multiplayer.API;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AllForOneGene
{
    public class JobDriver_CastAllForOne : JobDriver_CastAbility
    {
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Ability ability = ((Verb_CastAbility)job.verbToUse).ability;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !ability.CanApplyOn(job.targetA));
            var uiToil = ToilMaker.MakeToil("UI");
            uiToil.initAction = delegate
            {
                OpenUI();
            };
            yield return uiToil;
            yield return Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
        }

        [SyncMethod]
        private void OpenUI()
        {
            Find.WindowStack.Add(new Window_AllForOneGene(this.pawn, TargetA.Pawn));
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            job.ability?.Notify_StartedCasting();
        }
    }
}
