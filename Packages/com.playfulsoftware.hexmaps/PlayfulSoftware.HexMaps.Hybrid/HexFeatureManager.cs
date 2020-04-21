using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexFeatureManager : MonoBehaviour
    {
        public HexFeatureCollection[] farmCollections;
        public HexFeatureCollection[] plantCollections;
        public HexFeatureCollection[] urbanCollections;

        Transform m_Container;

        public void AddFeature(HexCell cell, Vector3 position)
        {
            var hash = HexMetrics.SampleHashGrid(position);
            var prefab = PickPrefab(urbanCollections, cell.urbanLevel, hash.a, hash.d);
            var otherPrefab = PickPrefab(farmCollections, cell.farmLevel, hash.b, hash.d);
            float usedHash = hash.a;
            if (prefab)
            {
                if (otherPrefab && hash.b < usedHash)
                {
                    prefab = otherPrefab;
                    usedHash = hash.b;
                }
            }
            else if (otherPrefab)
            {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
            otherPrefab = PickPrefab(plantCollections, cell.plantLevel, hash.c, hash.d);
            if (prefab)
            {
                if (otherPrefab && hash.c < usedHash)
                    prefab = otherPrefab;
            }
            else if (otherPrefab)
                prefab = otherPrefab;
            else
                return;

            var instance = Instantiate(prefab, m_Container, false);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        }

        public void Apply()
        {
        }

        public void Clear()
        {
            if (m_Container)
                Destroy(m_Container.gameObject);
            m_Container = new GameObject("FeaturesContainer").transform;
            m_Container.SetParent(transform, false);
        }

        Transform PickPrefab(
            HexFeatureCollection[] collection,
            int level, float hash, float choice)
        {
            Transform prefab = null;
            if (level <= 0)
                return prefab;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++)
            {
                if (hash < thresholds[i])
                {
                    prefab = collection[i].Pick(choice);
                    break;
                }
            }

            return prefab;
        }
    }
}