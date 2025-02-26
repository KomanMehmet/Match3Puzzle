using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace GamePlay.Grid
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private string[] tileAddressableKeys;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight  = 8;
        [SerializeField] private float tileSpacing = 1.1f;

        private void Start()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            float offsetX = ((gridWidth - 1) * tileSpacing) / 2f;
            float offsetY = ((gridHeight - 1) * tileSpacing) / 2f;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(x * tileSpacing - offsetX, y * tileSpacing - offsetY, 0f);
                    int randomIndex = Random.Range(0, tileAddressableKeys.Length);
                    string key = tileAddressableKeys[randomIndex];

                    Addressables.InstantiateAsync(key, pos, Quaternion.identity, transform);
                }
            }
        }
    }
}