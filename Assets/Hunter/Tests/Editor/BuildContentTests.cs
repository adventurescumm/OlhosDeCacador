using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Assemblies;

namespace OlhosDeCacador.Tests
{
    public sealed class BuildContentTests
    {
        Catalog catalog;
        MethodInfo validateStages;

        [SetUp]
        public void Setup()
        {
            catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("Hunter/catalog").text);
            // The build command belongs to Unity's default editor assembly.
            var type = CurrentAssemblies.GetLoadedAssemblies()
                .Select(a => a.GetType("OlhosDeCacador.Editor.HunterBuild"))
                .FirstOrDefault(t => t != null);
            Assert.IsNotNull(type, "HunterBuild must compile before running these integration tests.");
            validateStages = type.GetMethod("ValidateStages", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(validateStages);
        }

        [Test]
        public void CatalogWithOriginalFbxAndObjModelsPassesStageValidation()
        {
            var duck = Resources.Load<GameObject>("Hunter/Models/polyhaven-duck/rubber_duck_toy_1k");
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
    }
}
