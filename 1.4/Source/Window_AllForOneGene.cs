using HarmonyLib;
using Multiplayer.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AllForOneGene
{
    [StaticConstructorOnStartup]
    public static class MyExampleModCompat
    {
        static MyExampleModCompat()
        {
            if (!MP.enabled) return;
            MP.RegisterAll();
            MP.RegisterSyncWorker<Window_AllForOneGene>(SyncWindow_AllForOneGene, shouldConstruct: true);
            MP.RegisterPauseLock(delegate
            {
                return Find.WindowStack.IsOpen<Window_AllForOneGene>();
            });
        }
    
        private static void SyncWindow_AllForOneGene(SyncWorker sync, ref Window_AllForOneGene window)
        {
            if (sync.isWriting)
            {
                sync.Write(window.caster);
                sync.Write(window.target);
            }
            else
            {
                window.caster = sync.Read<Pawn>();
                window.target = sync.Read<Pawn>();
            }
        }
    }

    [HotSwappable]
    public class Window_AllForOneGene : Window
    {
        public Pawn caster;
        public Pawn target;

        public override Vector2 InitialSize => new Vector2(700, 485);

        public Window_AllForOneGene()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public Window_AllForOneGene(Pawn caster, Pawn target)
        {
            this.caster = caster;
            this.target = target;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public Vector2 scrollPosLeft;
        public Vector2 scrollPosRight;

        public override void DoWindowContents(Rect inRect)
        {
            var comp = caster.CurJob?.ability?.CompOfType<CompAbilityEffect_AllForOne>();
            if (comp is null)
            {
                this.Close(); 
                return;
            }
            var titleArea = new Rect(inRect.x, inRect.y, inRect.width, 50);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleArea, "All For One");
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            var rigthArea = new Rect(titleArea.x, titleArea.yMax, (inRect.width / 2f) - 15, inRect.height - titleArea.height - 60);
            DrawGeneArea(rigthArea, caster, casterGene: true, comp, ref scrollPosLeft);
            var leftArea = new Rect(rigthArea.xMax + 30, rigthArea.y, rigthArea.width, rigthArea.height);
            DrawGeneArea(leftArea, target, casterGene: false, comp, ref scrollPosRight);

            var cancelButton = new Rect((inRect.width / 3f) - (150), inRect.height - 32, 150, 32);
            if (Widgets.ButtonText(cancelButton, "Cancel"))
            {
                Cancel();
                this.Close();
            }
            var acceptButton = new Rect((inRect.width / 3f) + (150), inRect.height - 32, 150, 32);
            if (Widgets.ButtonText(acceptButton, "Start"))
            {
                Start();
                this.Close();
            }
        }

        [SyncMethod]
        private void Start()
        {
            CompAbilityEffect_AllForOne.ThrowRedSmoke(target.DrawPos, target.Map, 3f);
            AllForOne_DefOf.AllForOneCast.PlayOneShot(new TargetInfo(target.Position, target.Map));
        }

        public override void OnCancelKeyPressed()
        {
            base.OnCancelKeyPressed();
            Cancel();
        }

        [SyncMethod]
        private void Cancel()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
            caster.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Rand.PopState();
        }

        private void DrawGeneArea(Rect area, Pawn pawn, bool casterGene, CompAbilityEffect_AllForOne comp, ref Vector2 scrollPos)
        {
            var areaTitle = new Rect(area.x, area.y, area.width, 24);
            var scrollArea = new Rect(area.x, area.y + 24, area.width, area.height - 24);
            List<Gene> genes = pawn.genes.GenesListForReading.ToList();//.Where(x => x.def != AllForOne_DefOf.AllForOne).ToList();
            Widgets.Label(areaTitle, pawn.LabelShort);
            var viewRect = new Rect(scrollArea.x, scrollArea.y, scrollArea.width - 16, genes.Count * 24);
            Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
            for (var i = 0; i < genes.Count; i++)
            {
                var entryRect = new Rect(scrollArea.x, scrollArea.y + (i * 24), scrollArea.width - 24, 24);
                Text.Anchor = TextAnchor.MiddleLeft;
                var gene = genes[i];
                if (i % 2 == 1)
                {
                    Widgets.DrawLightHighlight(entryRect);
                }
                var geneIconRect = new Rect(entryRect.x, entryRect.y, 24, 24);
                Widgets.DefIcon(geneIconRect, gene.def);
                var infoCardRect = new Rect(geneIconRect.xMax, entryRect.y, 24, 24);
                Widgets.InfoCardButton(infoCardRect, gene.def);
                var nameRect = new Rect(infoCardRect.xMax, entryRect.y, viewRect.width - (3 * 24), 24);
                Text.WordWrap = false;
                Widgets.Label(nameRect, gene.LabelCap);
                Text.WordWrap = true;
                var targetList = casterGene ? comp.casterGenesToGive : comp.targetGenesToTake;
                bool enabled = targetList.Contains(gene);
                Widgets.Checkbox(nameRect.xMax, entryRect.y, ref enabled);
                if (enabled && !targetList.Contains(gene))
                {
                    if (casterGene)
                    {
                        comp.AddToCasterGenesToGiveList(gene);
                    }
                    else
                    {
                        comp.AddToTargetGenesToTakeList(gene);
                    }
                }
                else if (!enabled && targetList.Contains(gene))
                {
                    if (casterGene)
                    {
                        comp.RemoveFromCasterGenesToGiveList(gene);
                    }
                    else
                    {
                        comp.RemoveFromTargetGenesToTakeList(gene);
                    }
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndScrollView();
        }

        [SyncMethod] public static void AddToList(Gene gene, List<Gene> list) => list.Add(gene);
        [SyncMethod] public static void RemoveFromList(Gene gene, List<Gene> list) => list.Remove(gene);
    }
}
