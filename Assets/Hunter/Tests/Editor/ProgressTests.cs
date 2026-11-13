using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace OlhosDeCacador.Tests
{
    public sealed class ProgressTests
    {
        Catalog catalog;
        [SetUp] public void Setup() => catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("Hunter/catalog").text);
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
            Assert.IsTrue(p.Complete(map)); Assert.IsFalse(p.Complete(map)); Assert.IsFalse(p.Finished);
        }
        [Test] public void ResetMapDoesNotEraseOtherMaps()
        {
            var p = new Progress(catalog);
            foreach (var map in catalog.maps) { foreach (var target in map.Targets) p.Collect(target.id); p.Complete(map); }
            Assert.IsTrue(p.Finished); p.ResetMap(catalog.maps[1]);
            Assert.IsFalse(p.Finished); Assert.AreEqual(0, p.Count(catalog.maps[1]));
            Assert.AreEqual(6, p.Count(catalog.maps[0])); Assert.IsTrue(p.Solved(catalog.maps[2]));
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
        [Test] public void WrongMemoryInputResetsOnlyTheSequence()
        {
            var p = new Progress(catalog); p.Collect(catalog.maps[1].Targets.First().id);
            var s = new MemorySequence(new[] { 2, 0, 3, 1 }); Assert.IsTrue(s.Press(2));
            Assert.IsFalse(s.Press(1)); Assert.AreEqual(0, s.Position); Assert.AreEqual(1, p.Count(catalog.maps[1]));
            foreach (int v in new[] { 2, 0, 3, 1 }) Assert.IsTrue(s.Press(v));
            Assert.IsTrue(s.Complete); Assert.IsFalse(s.Press(1)); s.Reset(); Assert.AreEqual(0, s.Position);
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
