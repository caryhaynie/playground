using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Experimental.AssetImporters;

    [CustomEditor(typeof(HexMapAsset))]
    sealed class HexMapAssetEditor : Editor
    {

    }

    [ScriptedImporter(1, "map")]
    sealed class HexMapImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var map = ScriptableObject.CreateInstance<HexMapAsset>();
            ctx.AddObjectToAsset("Map", map);
            ctx.SetMainObject(map);
        }
    }
#endif // UNITY_EDITOR
    //[CreateAssetMenu(fileName = "HexMap", menuName = "HexMaps/Create Map", order = 0)]
    public sealed partial class HexMapAsset : ScriptableObject
    {
    }
}