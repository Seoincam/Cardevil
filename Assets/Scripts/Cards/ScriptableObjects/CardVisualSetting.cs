using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "CardVisualSetting", menuName = "Cards/Card Visual Setting")]
public class CardVisualSetting : ScriptableObject
{
    [Header("[ Common ]")]
    public float SelectOffset = 50f;


    [Header("[ HandBar ]")]
    public float EndDragTweenDuration = .2f;

    public float DrawInterval = .2f;
    public float DiscardInterval = .3f;
    public float ReviveInterval = .4f;


    [Header("[ Card ]")]
    public float MoveSpeedLimit = 4000;
    public float ClickDetectThreshold = .2f;


    [Header("[ Card Visual ]")]

    [Header("Follow")]
    public float FollowSpeed = 10;

    [Header("Select")]
    public float SelectScale = 1.25f;
    public float SelectScaleTweenDuration = .15f;
    public Ease SelectScaleEase = Ease.OutBack;

    [Header("Shadow")]
    public float ShadowOffset = 20;

    [Header("Curve")]
    public CurveParameters Curve;

    [Header("Discard")]
    public Transform DiscardPoint;
}
