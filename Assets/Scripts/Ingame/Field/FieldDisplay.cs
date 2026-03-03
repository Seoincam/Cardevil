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
            
            foreach (var move in moves)
            {
                var (resultTile, wrapped, reflected) = PlayerCharacter.GetTrueTileInDirection(field, currentTile, move,1);


                // 두개가 같은 내용이지만, 추후 바뀔 수 있으니 분리
                if (wrapped)
                {
                    TileVector airTile = currentTile.Coordinate + move.ToTileVector();
                    currentMoveRawPosition.Add(airTile);
                    break;
                }
                if (reflected)
                {
                    TileVector reflecterTile = currentTile.Coordinate + move.ToTileVector();
                    currentMoveRawPosition.Add(reflecterTile);
                    break;
                }
                
                currentMoveTiles.Add(resultTile);
                currentMoveRawPosition.Add(resultTile.Coordinate);
                currentTile = resultTile;
            }
        }


        private void OnDrawGizmos()
        {
            if (currentMoveTiles == null || currentMoveTiles.Count == 0)
            {
                return;
            }
            
            Vector3 prevPos = currentMoveTiles[0].transform.position;
            for (int i = 1; i < currentMoveTiles.Count; i++)
            {
                Vector3 currentPos = currentMoveTiles[i].transform.position;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(prevPos, currentPos);
                prevPos = currentPos;
            }
        }

        #endregion
    }
}