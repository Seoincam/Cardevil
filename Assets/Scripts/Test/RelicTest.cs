using Cardevil.Relic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class RelicTest : MonoBehaviour
{
    public static RelicTest instance;

    [SerializeField] List<RelicSO> _relics;

    private Text relicText;

    /*
    void Awake()
    {
        instance = this;
        relicText = GetComponent<Text>();

        var text = "[ 유물 ]\n";

        for (int i = 0; i < _relics.Count; i++)
        {
            if (_relics[i] == null)
                return;
            text += $"{i + 1}  |  {_relics[i].DisplayName}\n";
        }
        if (_relics.Count == 0)
            text += "없음";

        relicText.text = text;
    }

    void Start()
    {
        // 즉시 사용 유물
        foreach (var relic in GetRelics(RelicSO.TriggerTiming.OnAcquire))
            relic.Apply();

    }

    public List<RelicSO> GetRelics(RelicSO.TriggerTiming trigger)
    {
        return _relics
                .Where(r => r.Trigger == trigger)
                .ToList();
    }
    */
}
