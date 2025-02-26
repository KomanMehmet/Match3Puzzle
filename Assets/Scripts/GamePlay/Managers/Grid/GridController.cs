using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities.Enums;
using Random = UnityEngine.Random;
using Tile = GamePlay.Tiles.Tile;

namespace GamePlay.Managers.Grid
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private string[] tileAddressableKeys;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float tileSpacing = 1.1f;

        [SerializeField] private ScoreChangedEventChannel scoreChangedEventChannel;

        private Tile[,] _gridTiles;

        void Start()
        {
            _gridTiles = new Tile[gridWidth, gridHeight];
            GenerateGrid();
        }

        void GenerateGrid()
        {
            float offsetX = ((gridWidth - 1) * tileSpacing) / 2f;
            float offsetY = ((gridHeight - 1) * tileSpacing) / 2f;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(x * tileSpacing - offsetX, y * tileSpacing - offsetY, 0);
                    int randomIndex = Random.Range(0, tileAddressableKeys.Length);
                    string key = tileAddressableKeys[randomIndex];

                    int currentX = x;
                    int currentY = y;

                    var handle = Addressables.InstantiateAsync(key, pos, Quaternion.identity, transform);
                    handle.Completed += (operation) =>
                    {
                        Tile tile = operation.Result.GetComponent<Tile>();
                        if (tile != null)
                        {
                            tile.gridPosition = new Vector2Int(currentX, currentY);
                            _gridTiles[currentX, currentY] = tile;
                        }
                    };
                }
            }
        }

        public void OnTileSwiped(Tile tile, SwipeDirection direction)
        {
            Vector2Int targetPos = tile.gridPosition;
            switch (direction)
            {
                case SwipeDirection.Up:
                    targetPos += Vector2Int.up;
                    break;
                case SwipeDirection.Down:
                    targetPos += Vector2Int.down;
                    break;
                case SwipeDirection.Left:
                    targetPos += Vector2Int.left;
                    break;
                case SwipeDirection.Right:
                    targetPos += Vector2Int.right;
                    break;
            }

            if (targetPos.x < 0 || targetPos.x >= gridWidth || targetPos.y < 0 || targetPos.y >= gridHeight)
            {
                Debug.Log("Swipe grid sınırlarının dışında!");
                return;
            }

            Tile targetTile = _gridTiles[targetPos.x, targetPos.y];
            if (targetTile != null)
            {
                Vector2Int tempGridPos = tile.gridPosition;
                tile.gridPosition = targetTile.gridPosition;
                targetTile.gridPosition = tempGridPos;

                _gridTiles[tile.gridPosition.x, tile.gridPosition.y] = tile;
                _gridTiles[targetTile.gridPosition.x, targetTile.gridPosition.y] = targetTile;

                // DOTween ile animasyon başlatıyoruz
                AnimateSwap(tile, targetTile);
            }
        }

        // DOTween kullanarak iki tile'ın pozisyonlarını swap eden metot
        private void AnimateSwap(Tile a, Tile b)
        {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            float duration = 0.3f;

            // Tween'leri tamamlandıktan sonra CheckMatches çağrısını yapmak için OnComplete callback kullanıyoruz.
            a.transform.DOMove(posB, duration).SetEase(Ease.OutQuad);
            b.transform.DOMove(posA, duration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                // Swap animasyonu tamamlandıktan sonra eşleşmeleri kontrol et
                CheckMatches();
                RefillGrid();
            });
        }

        public void CheckMatches()
        {
            List<Tile> matchedTiles = new List<Tile>();

            // Yatay eşleşme kontrolü
            for (int y = 0; y < gridHeight; y++)
            {
                int matchCount = 1;
                for (int x = 1; x < gridWidth; x++)
                {
                    if (_gridTiles[x, y] != null && _gridTiles[x - 1, y] != null &&
                        _gridTiles[x, y].tileType == _gridTiles[x - 1, y].tileType)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int k = x - matchCount; k < x; k++)
                            {
                                if (!matchedTiles.Contains(_gridTiles[k, y]))
                                    matchedTiles.Add(_gridTiles[k, y]);
                            }
                        }

                        matchCount = 1;
                    }
                }

                if (matchCount >= 3)
                {
                    for (int k = gridWidth - matchCount; k < gridWidth; k++)
                    {
                        if (!matchedTiles.Contains(_gridTiles[k, y]))
                            matchedTiles.Add(_gridTiles[k, y]);
                    }
                }
            }

            // Dikey eşleşme kontrolü
            for (int x = 0; x < gridWidth; x++)
            {
                int matchCount = 1;
                for (int y = 1; y < gridHeight; y++)
                {
                    if (_gridTiles[x, y] != null && _gridTiles[x, y - 1] != null &&
                        _gridTiles[x, y].tileType == _gridTiles[x, y - 1].tileType)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int k = y - matchCount; k < y; k++)
                            {
                                if (!matchedTiles.Contains(_gridTiles[x, k]))
                                    matchedTiles.Add(_gridTiles[x, k]);
                            }
                        }

                        matchCount = 1;
                    }
                }

                if (matchCount >= 3)
                {
                    for (int k = gridHeight - matchCount; k < gridHeight; k++)
                    {
                        if (!matchedTiles.Contains(_gridTiles[x, k]))
                            matchedTiles.Add(_gridTiles[x, k]);
                    }
                }
            }

            if (matchedTiles.Count > 0)
            {
                int completedCount = 0;
                int basePoints = 100;
                int pointsToAdd = matchedTiles.Count * basePoints;
                
                scoreChangedEventChannel.RaiseEvent(pointsToAdd);
                
                foreach (Tile t in matchedTiles)
                {
                    t.transform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            _gridTiles[t.gridPosition.x, t.gridPosition.y] = null;
                            Destroy(t.gameObject);
                            completedCount++;
                            // Tüm matched tile'ların animasyonları tamamlandığında refill işlemini başlat
                            if (completedCount == matchedTiles.Count)
                            {
                                StartCoroutine(RefillGrid());
                            }
                        });
                }
            }
            else
            {
                Debug.Log("Eşleşme bulunamadı.");
            }
        }

        // Yok edilen tile'ların yerine yeni tile'ları getiren (ve varsa mevcut tile'ları aşağı kaydıran) coroutine
        private IEnumerator RefillGrid()
        {
            // Offset hesaplamaları: grid'in merkezini ayarlamak için.
            float offsetX = ((gridWidth - 1) * tileSpacing) / 2f;
            float offsetY = ((gridHeight - 1) * tileSpacing) / 2f;

            // Her sütun için işlemleri yapıyoruz.
            for (int x = 0; x < gridWidth; x++)
            {
                // 1. Adım: Mevcut sütunda boş hücreler için yukarıdaki tile'ları aşağı kaydır.
                // Aşağıdan yukarıya doğru kontrol ederek, boş olan hücreleri tespit edip,
                // o hücreden yukarıdaki ilk dolu tile'ı bulup aşağı taşıyoruz.
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_gridTiles[x, y] == null)
                    {
                        // Bu boş hücre için, y+1'den yukarıdaki ilk dolu tile'ı arıyoruz.
                        for (int y2 = y + 1; y2 < gridHeight; y2++)
                        {
                            if (_gridTiles[x, y2] != null)
                            {
                                // y2 konumundaki tile'yı y konumuna kaydırıyoruz.
                                Tile tileToDrop = _gridTiles[x, y2];
                                _gridTiles[x, y] = tileToDrop;
                                _gridTiles[x, y2] = null;
                                // Tile'ın gridPosition'ını güncelliyoruz.
                                tileToDrop.gridPosition = new Vector2Int(x, y);
                                // Hedef world pozisyonunu hesaplıyoruz.
                                Vector3 targetPos = new Vector3(x * tileSpacing - offsetX, y * tileSpacing - offsetY,
                                    0);
                                // DOTween ile tile'ı yumuşakça aşağı indiriyoruz.
                                tileToDrop.transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
                                // Bulduğumuz ilk dolu tile'ı kaydırdıktan sonra bu boşluk için aramayı durduruyoruz.
                                break;
                            }
                        }
                    }
                }

                // 2. Adım: Şimdi, o sütunda en üstte hala boş olan hücrelere yeni tile'lar ekleyelim.
                // Sütunun en üst hücreleri y ekseninde en yüksek index'tedir (gridHeight-1'den aşağıya doğru boşluk olabilir).
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    if (_gridTiles[x, y] == null)
                    {
                        // Yeni tile'ı eklemek için hedef pozisyonu hesaplıyoruz.
                        Vector3 targetPos = new Vector3(x * tileSpacing - offsetX, y * tileSpacing - offsetY, 0);
                        // Yeni tile'ı spawn edeceğimiz pozisyon, hedefin biraz üstünde olsun; böylece düşüş animasyonu gözlemlensin.
                        Vector3 spawnPos = targetPos + Vector3.up * tileSpacing;
                        int currentX = x;
                        int currentY = y;
                        int randomIndex = Random.Range(0, tileAddressableKeys.Length);
                        string key = tileAddressableKeys[randomIndex];

                        var handle = Addressables.InstantiateAsync(key, spawnPos, Quaternion.identity, transform);
                        handle.Completed += (operation) =>
                        {
                            Tile newTile = operation.Result.GetComponent<Tile>();
                            if (newTile != null)
                            {
                                newTile.gridPosition = new Vector2Int(currentX, currentY);
                                _gridTiles[currentX, currentY] = newTile;
                                // Yeni tile önce scale olarak da giriş animasyonu verebiliriz.
                                newTile.transform.localScale = Vector3.zero;
                                newTile.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                                // Düşme animasyonu: spawnPos'dan targetPos'a hareket.
                                newTile.transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
                            }
                        };
                    }
                }
            }

            // Yeni tile'ların animasyonları tamamlanması için kısa bir bekleme.
            yield return new WaitForSeconds(0.3f);
            // İsteğe bağlı: Zincirleme eşleşmeler için tekrar CheckMatches() çağır.
            CheckMatches();
        }
    }
}