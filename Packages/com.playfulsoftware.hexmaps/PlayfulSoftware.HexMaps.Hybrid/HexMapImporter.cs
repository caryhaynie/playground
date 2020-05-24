#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    [ScriptedImporter(1, "map")]
    sealed class HexMapImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var root = new GameObject("root");
            var grid = new GameObject("Hex Map");
            grid.transform.SetParent(root.transform, false);
            ctx.AddObjectToAsset("Hex Map", root);
            ctx.SetMainObject(root);
        }
    }
}
#endif // UNITY_EDITOR