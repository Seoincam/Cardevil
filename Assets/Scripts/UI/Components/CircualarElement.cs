using UnityEngine;

namespace Cardevil.UI.Components
{
    

/// <summary>
/// 원형 UI 요소를 나타내는 인터페이스
/// 내부 값은 rawAngle로 저장.
/// </summary>
public class CircularElement : MonoBehaviour, ICircularUI
{
    public Transform center;
    public float radius = 1f;
    [SerializeField,Range(0,360)]private float absAngle = 0f;
    [SerializeField,Range(0,1)] private float value = 0f;
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float endAngle = 360f;
    public bool doRotate = true;
    public bool isClockwise = true;
    
    [SerializeField] private float globalAngle = 0f;
    public void Initialize(float startAngle, float endAngle, bool clockwise)
    {
        this.startAngle = startAngle;
        this.endAngle = endAngle;
        this.isClockwise = clockwise;
    }
    
    public float AbsAngle
    {
        get => absAngle;
        set
        {
            absAngle = value;
            globalAngle = isClockwise ? 360 - AbsAngle : AbsAngle;
            if(Mathf.Approximately(endAngle , startAngle))
            {
                this.value = 0;
            }
            else
            {
                this.value = (absAngle - startAngle) / (endAngle - startAngle);
            }
            UpdatePos();
            UpdateRotation();
        }
    }

    public float Value
    {
        get => value;
        set
        {
            AbsAngle = startAngle + (endAngle - startAngle) * value;
        }
    }

    private void UpdatePos()
    {
        var x = center.position.x + radius * Mathf.Cos(globalAngle * Mathf.Deg2Rad);
        var y = center.position.y + radius * Mathf.Sin(globalAngle * Mathf.Deg2Rad);
        transform.position = new Vector3(x, y, 0);
    }
    
    private void UpdateRotation()
    {
        if (doRotate)
        {
            transform.rotation = Quaternion.Euler(0, 0, globalAngle);
        }
    }

    #if UNITY_EDITOR
    [Header("Editor Only")]
    public bool useAngle = false;

    private ICircularUI _circularUIImplementation;

    private void OnValidate()
    {
        if (useAngle)
        {
            AbsAngle = absAngle;
        }
        else
        {
            Value = value;
        }
    }
    #endif
}

}