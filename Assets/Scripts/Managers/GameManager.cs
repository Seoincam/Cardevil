using System;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Ingame.Field;
using Cardevil.Ingame.Entities;

public class GameManager
{

    public Field field;
    public int turnOrder = 0;
    private Entity entity; // 임시 플레이어
    public void Clear()
    {
 
     }

    //게임 상태를 나눠서 상태에 따라 스크립트들이 돌아가게 함
    public enum GameState
    {
        
    }
    public GameState currentState;
    //플레이어 죽을 때 실행시킬 함수
    public void PlayerDied()
    {
       
    }
    //인게임 데이터 초기화 
    public void GameStart()
    {

    }

    public void StageStart()
    {
        turnOrder = 0;
    }
 

}
