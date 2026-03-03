using Cardevil.Attributes;
using Cardevil.Card.InStage;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Root;
using Cardevil.Events.ExecEvents;
using Cardevil.Ingame.Player;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cardevil.Ingame.Field
{
    public class FieldDisplay : MonoBehaviour
    {
        [SerializeField] private Field field;
        

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            ExecEventBus<SelectedMoveChangedArgs>.RegisterStatic(0, OnSelectedMoveChanged);
        }
        
        private void OnDisable()
        {
            ExecEventBus<SelectedMoveChangedArgs>.UnregisterStatic(OnSelectedMoveChanged);
        }

        
        
        #region Line Display Logic
        // TODO : 별도 클래스 분리
        

        [SerializeField] private LineRenderer currentMoveLineRenderer;
        [VisibleOnly, SerializeField] private List<Tile> currentMoveTiles = new List<Tile>();
        /// <summary>
        /// 플레이어가 선택한 이동 경로의 원시 타일 좌표 리스트.
        /// currentMoveTiles는 실제 타일 오브젝트 리스트인 반면, currentMoveRawPosition은 타일 좌표 리스트입니다.
        /// wrap이 일어난 경우 개수가 다를 수 있습니다.
        /// </summary>
        [VisibleOnly, SerializeField] private List<TileVector> currentMoveRawPosition = new List<TileVector>();
        
        private async UniTask OnSelectedMoveChanged(SelectedMoveChangedArgs eventArgs, CancellationToken cancellationToken)
        {
            if (!eventArgs.ShouldShow)
            {
                currentMoveLineRenderer.positionCount = 0;
                currentMoveTiles.Clear();
                currentMoveRawPosition.Clear();
                return;
            }

            var root = CardevilCore.Instance.GameFlow.Stage;
            var field = root.Field;
            var player = root.Player;
            var moves = eventArgs.Directions;
            Tile currentTile = player.Entity.CurrentTile;
            
            
            currentMoveTiles.Clear();
            currentMoveRawPosition.Clear();
            
            currentMoveTiles.Add(currentTile);
            currentMoveRawPosition.Add(currentTile.Coordinate);
            
            List<(float distance, Vector3 Position, Color color)> positions = new ();
            positions.Add((0, currentTile.transform.position, Color.clear));
            
            
            float accumulatedDistance = 0f;
            
            void AddPosition(Vector3 position, Color color)
            {
                var (prevDistance, prevPosition, prevColor) = positions[positions.Count - 1];
                Debug.DrawLine(prevPosition, position, color, 1f);
                if (prevColor != color)
                {
                    // 이전 위치에서 살짝 떨어진 위치에 색이 바뀌는 지점을 표시
                    Vector3 colorChangePosition = Vector3.Lerp(prevPosition, position, 0.01f);
                    positions.Add((prevDistance + Vector3.Distance(prevPosition, colorChangePosition), colorChangePosition, color));
                    accumulatedDistance += Vector3.Distance(prevPosition, colorChangePosition);
                    prevPosition = colorChangePosition;
                    prevDistance += Vector3.Distance(prevPosition, colorChangePosition);
                    prevColor = color;
                }
                
                accumulatedDistance += Vector3.Distance(prevPosition, position);
                positions.Add((accumulatedDistance, position, color));
                currentMoveRawPosition.Add(currentTile.Coordinate);
            }
            
            for(int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                var (resultTile, wrapped, reflected) = PlayerCharacter.GetTrueTileInDirection(field, currentTile, move,1);
                
                // 두개가 같은 내용이지만, 추후 바뀔 수 있으니 분리
                if (wrapped)
                {
                    TileVector airTile = currentTile.Coordinate + move.ToTileVector();
                    AddPosition(field.GetTileCenterWorld(airTile), Color.blue);
                    break;
                }
                if (reflected)
                {
                    TileVector reflectorTile = currentTile.Coordinate + move.ToTileVector();
                    AddPosition(field.GetTileCenterWorld(reflectorTile), Color.red);
                    break;
                }
                
                currentMoveTiles.Add(resultTile);
                AddPosition(resultTile.transform.position, Color.green);
                currentTile = resultTile;
            }
            var gradient = new Gradient();
            List<GradientColorKey> colorKeys = new List<GradientColorKey>();
            for(int i = 0; i < positions.Count; i++)
            {
                var (distance, position, color) = positions[i];
                colorKeys.Add(new GradientColorKey(color, distance / accumulatedDistance));
            }
            currentMoveLineRenderer.positionCount = positions.Count;
            currentMoveLineRenderer.SetPositions(positions.ConvertAll(p => p.Position).ToArray());
            gradient.SetKeys(colorKeys.ToArray(), new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            currentMoveLineRenderer.colorGradient = gradient;
            
        }
        

        #endregion
    }
}