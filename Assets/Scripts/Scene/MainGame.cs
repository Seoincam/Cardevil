using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainGame : BaseScene   // MainGame Ŭ������ BaseScene Ŭ������ ����� ������� ���� ���� �� �ʿ��� �ʱ�ȭ �۾��� �����ϴ� Ŭ����
{


    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

    protected override void Init()
    {
       
        base.Init();
   
    }

    private void Start()
    {
        Init();

        // Managers.UI.ShowPopUpUI<S1_PopUp>();
        Managers.Sound.Play("Sounds/BGM/Main_Bgm",Define.Sound.BGM);
        Managers.Game.GameStart();
        Managers.UI.ShowPopUpUI<SlotMachine>();
    }
    public void Option()
    {

    }
}
