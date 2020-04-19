using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class HexFeatureManager : MonoBehaviour
    {
        public Transform[] urbanPrefabs;

        Transform m_Container;

        public void AddFeature(HexCell cell, Vector3 position)
        {
            var hash = HexMetrics.SampleHashGrid(position);
            var prefab = PickPrefab(cell.urbanLevel, hash.x);
            if (!prefab)
                return;

            var instance = Instantiate(prefab, m_Container, false);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.y, 0f);
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

        Transform PickPrefab(int level, float hash)
        {
            Transform prefab = null;
            if (level <= 0)
                return prefab;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++)
            {
                if (hash < thresholds[i])
                {
                    prefab = urbanPrefabs[i];
                    break;
                }
            }

            return prefab;
        }
    }
}