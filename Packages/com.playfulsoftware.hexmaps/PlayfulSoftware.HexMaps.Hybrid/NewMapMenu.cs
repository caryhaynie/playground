using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class NewMapMenu : MonoBehaviour
    {
        public HexGrid hexGrid;
        public HexMapCamera hexMapCamera;

        void CreateMap(int x, int z)
        {
            if (!hexGrid)
                return;
            hexGrid.CreateMap(x, z);
            if (hexMapCamera)
                hexMapCamera.ValidatePosition();
            Close();
        }

        public void Open()
        {
            if (hexMapCamera)
                hexMapCamera.Lock();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (hexMapCamera)
                hexMapCamera.Unlock();
        }

        public void CreateSmallMap() => CreateMap(20, 15);
        public void CreateMediumMap() => CreateMap(40, 30);
        public void CreateLargeMap() => CreateMap(80, 60);
    }
}