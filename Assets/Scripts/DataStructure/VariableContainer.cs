    
using Cardevil.Core;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


namespace Cardevil.DataStructure
{


/// <summary>
/// 변수 컨테이너.
/// 
/// </summary>
[Serializable]
public class VariableContainer : ICopy<VariableContainer>
{
    [FormerlySerializedAs("arguments")] [SerializeField]private List<KeyVariablePair> variables = new List<KeyVariablePair>();
    public List<KeyVariablePair> Variables => variables;
    public void Clear() => variables.Clear();
    
    /// <summary>
    /// string 키값은 불안정 하므로, enum 타입을 사용하여 변수 타입을 정의
    /// </summary>
    public enum CardVariableKey
    {
        None,
        
        // 필요한 변수 타입을 여기에 추가.
        
    }

    
    [Serializable]
    public class KeyVariablePair : ICloneable
    {
        [CanBeNull] public string key; 
        [CanBeNull] public string stringValue;
        [CanBeNull] public int intValue;
        [CanBeNull] public float floatValue;
        [Obsolete("해당 값은 Serialization에서 사용하지 않습니다. 대신 stringValue, intValue, floatValue를 사용하세요.")] 
        // ReSharper disable once InconsistentNaming
        [CanBeNull,SerializeReference] public object objectValue;
        public object Clone()
        {
            return new KeyVariablePair
            {
                key = key,
                stringValue = stringValue,
                intValue = intValue,
                floatValue = floatValue
            };
        }
    }
    
    public bool HasVariable(CardVariableKey cardVariableKey) => HasVariable(cardVariableKey.ToString());
    public bool HasVariable(string key)
    {
        foreach (var agument in variables)
        {
            if (agument.key == key)
            {
                return true;
            }
        }
        return false;
    }
    
    public KeyVariablePair GetVariable(CardVariableKey cardVariableKey)
    {
        return GetVariable(cardVariableKey.ToString());
    }
    public KeyVariablePair GetVariable(string key)
    {
        foreach (var agument in variables)
        {
            if (agument.key == key)
            {
                return agument;
            }
        }
        return null;
    }
    
    public bool TryGetVariable(CardVariableKey cardVariableKey, out KeyVariablePair variablePair)
    {
        return TryGetVariable(cardVariableKey.ToString(), out variablePair);
    }
    public bool TryGetVariable(string key, out KeyVariablePair variablePair)
    {
        variablePair = GetVariable(key);
        return variablePair != null;
    }
    public bool TryGetString(CardVariableKey cardVariableKey, out string value)
    {
        return TryGetString(cardVariableKey.ToString(), out value);
    }
    public bool TryGetString(string key, out string value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            value = string.Empty;
            return false;
        }
        value = variable.stringValue;
        return true;
    }
    public bool TryGetInt(CardVariableKey cardVariableKey, out int value)
    {
        return TryGetInt(cardVariableKey.ToString(), out value);
    }
    public bool TryGetInt(string key, out int value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            value = 0;
            return false;
        }
        value = variable.intValue;
        return true;
    }
    public bool TryGetFloat(CardVariableKey cardVariableKey, out float value)
    {
        return TryGetFloat(cardVariableKey.ToString(), out value);
    }
    public bool TryGetFloat(string key, out float value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            value = 0f;
            return false;
        }
        value = variable.floatValue;
        return true;
    }
    
    
    public string GetString(CardVariableKey cardVariableKey) => GetStringDefault(cardVariableKey.ToString(), string.Empty);
    public string GetString(string key) => GetStringDefault(key, string.Empty);
    public string GetStringDefault(string key, string defaultValue)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            return defaultValue;
        }
        return variable.stringValue;
    }
    
    public int GetInt(CardVariableKey cardVariableKey) => GetIntDefault(cardVariableKey.ToString(), 0);
    public int GetInt(string key) => GetIntDefault(key, 0);
    public int GetIntDefault(string key, int defaultValue)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            return defaultValue;
        }
        return variable.intValue;
    }
    
    public void SetString(CardVariableKey cardVariableKey, string value)
    {
        SetString(cardVariableKey.ToString(), value);
    }
    public void SetString(string key, string value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair();
            variable.key = key;
            variables.Add(variable);
        }
        variable.stringValue = value;
    }
    public void SetInt(CardVariableKey cardVariableKey, int value)
    {
        SetInt(cardVariableKey.ToString(), value);
    }
    public void SetInt(string key, int value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair();
            variable.key = key;
            variables.Add(variable);
        }
        variable.intValue = value;
    }
    
    public void AddInt(CardVariableKey cardVariableKey, int value)
    {
        AddInt(cardVariableKey.ToString(), value);
    }
    public void AddInt(string key, int value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair();
            variable.key = key;
            variables.Add(variable);
        }
        variable.intValue += value;
    }
    
    public void SetFloat(CardVariableKey cardVariableKey, float value)
    {
        SetFloat(cardVariableKey.ToString(), value);
    }
    public void SetFloat(string key, float value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair();
            variable.key = key;
            variables.Add(variable);
        }
        variable.floatValue = value;
    }
    public float GetFloat(CardVariableKey cardVariableKey) => GetFloatDefault(cardVariableKey.ToString(), 0f);
    public float GetFloat(string key) => GetFloatDefault(key, 0f);
    public float GetFloatDefault(string key, float defaultValue)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            return defaultValue;
        }
        return variable.floatValue;
    }
    
    public void AddFloat(CardVariableKey cardVariableKey, float value)
    {
        AddFloat(cardVariableKey.ToString(), value);
    }
    public void AddFloat(string key, float value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair();
            variable.key = key;
            variables.Add(variable);
        }
        variable.floatValue += value;
    }
    
    
    public void CopyFrom(VariableContainer target)
    {
        Clear();
        foreach (var argument in target.variables)
        {
            variables.Add((KeyVariablePair)argument.Clone());
        }
    }
    
    public void CopyTo(VariableContainer target)
    {
        target.CopyFrom(this);
    }
    
    public VariableContainer Clone()
    {
        var clone = new VariableContainer();
        clone.CopyFrom(this);
        return clone;
    }
}

}