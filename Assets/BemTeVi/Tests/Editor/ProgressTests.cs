using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BemTeVi.Tests
{
    public sealed class ProgressTests
    {
        Catalog catalog;
        [SetUp] public void Setup() => catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("BemTeVi/catalog").text);
        [Test] public void RepeatedAndUnknownClicksNeverInflateProgress()
        {
            var p = new Progress(catalog); var id = catalog.maps[0].Targets.First().id;
            Assert.IsTrue(p.Collect(id)); Assert.IsFalse(p.Collect(id)); Assert.IsFalse(p.Collect("unknown"));
            Assert.AreEqual(1, p.Count(catalog.maps[0]));
        }
        [Test] public void AMapCannotBeCompletedBeforeAllItsTargetsAreFound()
        {
            var p = new Progress(catalog); var map = catalog.maps[0];
            Assert.IsFalse(p.Complete(map));
            foreach (var target in map.Targets) p.Collect(target.id);
            Assert.IsTrue(p.Complete(map)); Assert.AreEqual(1, map.currentRound); Assert.IsFalse(p.Complete(map)); Assert.IsFalse(p.Finished);
        }
        [Test] public void ResetMapDoesNotEraseOtherMaps()
        {
            var p = new Progress(catalog);
            foreach (var map in catalog.maps) for (int round = 0; round < 3; round++) { foreach (var target in map.Targets.ToArray()) p.Collect(target.id); p.Complete(map); }
            Assert.IsTrue(p.Finished); p.ResetMap(catalog.maps[1]);
            Assert.IsFalse(p.Finished); Assert.AreEqual(0, p.Count(catalog.maps[1]));
            Assert.AreEqual(18, p.Count(catalog.maps[0])); Assert.IsTrue(p.Solved(catalog.maps[2]));
        }
        [Test] public void LoadingFiltersDuplicateUnknownAndImpossibleCompletionEntries()
        {
            var data = new SaveData(); string id = catalog.maps[0].Targets.First().id;
            data.found.AddRange(new[] { id, id, "missing" }); data.solved.AddRange(new[] { catalog.maps[0].id, "missing" });
            var p = new Progress(catalog, data);
            Assert.AreEqual(1, p.Data.found.Count); Assert.AreEqual(0, p.Data.solved.Count);
        }
        [Test] public void InvalidSaveSchemaCanBeCaughtByRecoveryLayer()
        {
            Assert.Throws<ArgumentException>(() => new Progress(catalog, new SaveData { schema = 99 }));
            Assert.Throws<ArgumentException>(() => new Progress(catalog, new SaveData { found = null }));
        }
        [Test] public void SaveRoundTripPreservesProgress()
        {
            var p = new Progress(catalog); p.Collect(catalog.maps[0].Targets.First().id);
            var copy = new Progress(catalog, JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(p.Data)));
            CollectionAssert.AreEqual(p.Data.found, copy.Data.found);
        }
        [Test] public void MechanismsNeedTheCorrectPieceButDoNotBlockExploration()
        {
            var p = new Progress(catalog); var map = catalog.maps[0]; var gate = map.gates[0];
            Assert.IsTrue(p.CanVisit(map, map.rooms[0].id)); Assert.IsTrue(p.CanVisit(map, gate.roomId));
            Assert.IsFalse(p.Use(map, gate.id, gate.tokenId));
            p.Collect(gate.tokenId);
            foreach (var t in map.rooms[gate.sourceRoom].objects.Where(t => !string.IsNullOrEmpty(t.id))) p.Collect(t.id);
            Assert.IsFalse(p.Use(map, gate.id, "wrong")); Assert.IsTrue(p.Use(map, gate.id, gate.tokenId));
            Assert.IsTrue(p.CanVisit(map, gate.roomId)); Assert.IsFalse(p.Use(map, gate.id, gate.tokenId));
            Assert.IsTrue(p.Has(gate.tokenId));
        }
        [Test] public void OldSavesKeepFindsAndInvalidPassagesAreDiscarded()
        {
            var map = catalog.maps[0]; var data = new SaveData { opened = null };
            data.found.Add(map.Targets.First().id);
            var p = new Progress(catalog, data); Assert.AreEqual(1, p.Count(map)); Assert.IsEmpty(p.Data.opened);
            data.opened.Add(map.gates[0].id); data.opened.Add("missing");
            Assert.IsEmpty(new Progress(catalog, data).Data.opened);
        }
        [Test] public void SavedPassagesSurviveReloadAndResetWithTheirOwnWorld()
        {
            var p = new Progress(catalog);
            foreach (var map in catalog.maps)
            {
                foreach (var t in map.Targets) p.Collect(t.id);
                foreach (var g in map.gates) Assert.IsTrue(p.Use(map, g.id, g.tokenId));
            }
            p = new Progress(catalog, JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(p.Data)));
            Assert.AreEqual(6, p.Data.opened.Count); p.ResetMap(catalog.maps[0]);
            Assert.AreEqual(4, p.Data.opened.Count); Assert.AreEqual(18, p.Count(catalog.maps[1]));
        }
        [Test] public void WrongMemoryInputResetsOnlyTheSequence()
        {
            var p = new Progress(catalog); p.Collect(catalog.maps[1].Targets.First().id);
            var s = new MemorySequence(new[] { 2, 0, 3, 1 }); Assert.IsTrue(s.Press(2));
            Assert.IsFalse(s.Press(1)); Assert.AreEqual(0, s.Position); Assert.AreEqual(1, p.Count(catalog.maps[1]));
            foreach (int v in new[] { 2, 0, 3, 1 }) Assert.IsTrue(s.Press(v));
            Assert.IsTrue(s.Complete); Assert.IsFalse(s.Press(1)); s.Reset(); Assert.AreEqual(0, s.Position);
        }
        [Test] public void PhysicalPassagesRejectTeleportAndAllowExploration()
        {
            var p = new Progress(catalog);
            foreach (var map in catalog.maps)
            {
                Assert.IsTrue(p.CanTraverse(map, map.junction, map.rooms[0].id));
                Assert.IsFalse(p.CanTraverse(map, map.junction, map.rooms[1].id));
                Assert.IsTrue(p.CanTraverse(map, map.junction, map.rooms[2].id));
                Assert.IsFalse(p.CanTraverse(map, map.rooms[1], "quarto"));
                Assert.IsTrue(p.CanTraverse(map, map.junction, "quarto"));
            }
        }
        [Test] public void EveryPhysicalPassageHasAReturnAndWorksAfterProgression()
        {
            var p = new Progress(catalog);
            foreach (var map in catalog.maps)
            {
                foreach (var target in map.Targets) p.Collect(target.id);
                foreach (var gate in map.gates) p.Use(map, gate.id, gate.tokenId);
                var scenes = map.rooms.Concat(new[] { map.junction, map.challenge }).ToArray();
                foreach (var scene in scenes)
                foreach (var exit in scene.objects.Where(x => x.action == "exit"))
                {
                    Assert.IsTrue(p.CanTraverse(map, scene, exit.destination));
                    if (exit.destination != "quarto")
                        Assert.IsTrue(scenes.First(r => r.id == exit.destination).objects.Any(x => x.action == "exit" && x.destination == scene.id));
                }
            }
        }
        [Test] public void LockAndMatchingValidateCompleteSolutions()
        {
            Assert.IsFalse(PuzzleRules.Combination(new[] { 2, 4 }, new[] { 2, 4, 3 }));
            Assert.IsTrue(PuzzleRules.Combination(new[] { 2, 4, 3 }, new[] { 2, 4, 3 }));
            Assert.IsFalse(PuzzleRules.Match(0, 0, new[] { 1, 2, 0 }));
            Assert.IsTrue(PuzzleRules.Match(1, 0, new[] { 1, 2, 0 }));
            Assert.IsFalse(PuzzleRules.Match(0, -1, new[] { 1, 2, 0 }));
        }
    }
}
