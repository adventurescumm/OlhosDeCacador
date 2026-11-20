using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Assemblies;

namespace BemTeVi.Tests
{
    public sealed class BuildContentTests
    {
        Catalog catalog;
        MethodInfo validateStages;

        [SetUp]
        public void Setup()
        {
            catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("BemTeVi/catalog").text);
            // The build command belongs to Unity's default editor assembly.
            var type = CurrentAssemblies.GetLoadedAssemblies()
                .Select(a => a.GetType("BemTeVi.Editor.BemTeViBuild"))
                .FirstOrDefault(t => t != null);
            Assert.IsNotNull(type, "BemTeViBuild must compile before running these integration tests.");
            validateStages = type.GetMethod("ValidateStages", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(validateStages);
        }

        [Test]
        public void CatalogWithOriginalFbxAndObjModelsPassesStageValidation()
        {
            var duck = Resources.Load<GameObject>("BemTeVi/Models/polyhaven-duck/rubber_duck_toy_1k");
            Assert.IsNotNull(duck, "Wait for model import to finish.");
            StringAssert.EndsWith(".fbx", AssetDatabase.GetAssetPath(duck));
            Assert.DoesNotThrow(() => validateStages.Invoke(null, new object[] { catalog }));
        }

        [Test]
        public void MissingModelStillBlocksValidation()
        {
            catalog.hub.objects[0].asset = "__missing_model_for_validation_test__";
            var failure = Assert.Throws<TargetInvocationException>(
                () => validateStages.Invoke(null, new object[] { catalog }));
            Assert.IsInstanceOf<BuildFailedException>(failure.InnerException);
            StringAssert.Contains("__missing_model_for_validation_test__", failure.InnerException.Message);
        }

        [Test]
        public void SourceMaterialsHaveNoTexturesInLightingOrSurfaceMapSlots()
        {
            var type = CurrentAssemblies.GetLoadedAssemblies()
                .Select(a => a.GetType("BemTeVi.Editor.BemTeViMaterialRepair")).First(t => t != null);
            var validate = type.GetMethod("ValidateDiffuseTextureSlots", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(validate);
            var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Resources/BemTeVi/Materials" });
            Assert.Greater(guids.Length, 0);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.IsNotNull(material, path);
                Assert.DoesNotThrow(() => validate.Invoke(null, new object[] { material, path }), path);
            }
        }

        [Test]
        public void DiscoveryAndReloadKeepTheObjectVisibleAndClickable()
        {
            var type = CurrentAssemblies.GetLoadedAssemblies().Select(a => a.GetType("BemTeVi.RoomView")).First(t => t != null);
            var progress = new Progress(catalog); var map = catalog.maps[0]; var room = map.rooms[0];
            string id = room.Targets.First().id;
            for (int visit = 0; visit < 2; visit++)
            {
                var view = System.Activator.CreateInstance(type, map, room, progress, true);
                try
                {
                    var targets = (System.Collections.IDictionary)type.GetField("Targets").GetValue(view);
                    var target = (Component)targets[id];
                    Assert.IsNotNull(target);
                    var position = target.transform.localPosition; var scale = target.transform.localScale;
                    if (visit == 0) { progress.Collect(id); target.GetType().GetMethod("Celebrate").Invoke(target, null); }
                    Assert.IsTrue((bool)target.GetType().GetProperty("Collected").GetValue(target));
                    Assert.IsTrue(target.gameObject.activeInHierarchy);
                    Assert.IsTrue(target.GetComponentsInChildren<Renderer>().Any(r => r.enabled));
                    Assert.IsTrue(target.GetComponentsInChildren<Collider>().Any(c => c.enabled));
                    Assert.AreEqual(position, target.transform.localPosition); Assert.AreEqual(scale, target.transform.localScale);
                    Assert.IsFalse(progress.Collect(id));
                }
                finally { ((System.IDisposable)view).Dispose(); }
            }
        }

        [TestCase("sphere")]
        [TestCase("cylinder")]
        public void FlatArchitecturalDisksDoNotCreateInvisibleWalls(string shape)
        {
            var type = CurrentAssemblies.GetLoadedAssemblies().Select(a => a.GetType("BemTeVi.RoomView")).First(t => t != null);
            var room = new RoomData { id = "collider_check", name = "Disco de conferência", objects = System.Array.Empty<PropData>(),
                geometry = new[] { new PrimitiveData { name = "Janela plana", shape = shape, x = 50, z = 3,
                    width = 2, height = shape == "sphere" ? 2 : .1f, depth = shape == "sphere" ? .1f : 2,
                    tiltX = shape == "cylinder" ? 90 : 0, color = "#FFFFFF" } } };
            var view = System.Activator.CreateInstance(type, catalog.maps[0], room, new Progress(catalog), true);
            try
            {
                Physics.SyncTransforms();
                var ray = new Ray(new Vector3(50, 0, 0), Vector3.forward);
                Assert.IsFalse(Physics.Raycast(ray, 2.5f), "The flattened disk must not inflate towards the camera.");
                Assert.IsTrue(Physics.Raycast(ray, out var hit, 4));
                Assert.That(hit.distance, Is.EqualTo(2.95f).Within(.03f));
            }
            finally { ((System.IDisposable)view).Dispose(); }
        }
    }
}
