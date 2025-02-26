using GamePlay.Managers.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.Enums;

namespace GamePlay.Tiles
{
    public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
         public TileType tileType;
        public Vector2Int gridPosition;
        [SerializeField] float swipeThreshold = 50f;

        
        private Vector2 _pointerDownPos;
        private Vector2 _pointerUpPos;

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPos = eventData.position;
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _pointerUpPos = eventData.position;
            DetectSwipe();
        }

        private void DetectSwipe()
        {
            Vector2 delta = _pointerUpPos - _pointerDownPos;

            if (delta.magnitude >= swipeThreshold)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    if(delta.x > 0)
                        OnSwiped(SwipeDirection.Right);
                    else
                        OnSwiped(SwipeDirection.Left);
                }
                else
                {
                    if(delta.y > 0)
                        OnSwiped(SwipeDirection.Up);
                    else
                        OnSwiped(SwipeDirection.Down);
                }
            }
        }

        public void OnSwiped(SwipeDirection direction)
        {
            Debug.Log($"Tile {gridPosition} swiped {direction}");
            GridController gridController = FindObjectOfType<GridController>();
            if (gridController != null)
            {
                gridController.OnTileSwiped(this, direction);
            }
        }
    }
}