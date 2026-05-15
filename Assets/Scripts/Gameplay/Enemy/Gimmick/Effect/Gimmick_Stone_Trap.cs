using Cardevil.Core.Bootstrap;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Entities;
using Cardevil.Gameplay.Field;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy.Gimmick.Effect
{
    /// <summary>
    /// "# 돌무더기 (플레이어가 해당 칸으로 도착 시, 이동 과정 중 바로 직전에 위치했던 칸으로 되돌리는 기믹)
    ///* 돌무더기 = 내가 해당 장소로 이동하게 되면, 그 장소로 이동한 방향의 역방향으로 1칸 이동시키고 파괴됨. (즉, 스프링처럼 백도 시키는거)"
    /// </summary>
    public class Gimmick_Stone_Trap : IGimmick
    {
        private Enemy _targetEnemy;
        
        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            
            Debug.LogError("@김대윤 님께서 Field가져오고 랜덤 설정하도록 해주세요.");

            int placeCount = (int) enemy.baseMobBossData.GimmickValue[0];
            int rockCount = (int) enemy.baseMobBossData.GimmickValue[1];
            
            var pm = CardevilCore.Pool;
            var pile = pm.Get<RockPile>("RockPile");
            pile.Init(rockCount);
            
            TileVector? summonPos;
            for(int i = 0; i < placeCount; i++)
            {
                // TODO : 소환위치 랜덤으로 선정
                summonPos = enemy.field.GetRandomTileCoordinateWithoutPlayer();
                if (!summonPos.HasValue)
                {
                    Debug.LogError("No valid tile found to summon the rock pile.");
                    return;
                }
                
                enemy.field.SummonEntityComponent(pile, summonPos.Value);
            }

            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");

            // TODO : 돌무더기 설치
            return;
        }

        // 구독해제
        public void Remove()
        {
         
        }
    }
}
