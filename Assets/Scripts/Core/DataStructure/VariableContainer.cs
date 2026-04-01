using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Core.DataStructure
{
    
    /// <summary>
    /// string 키값은 불안정 하므로, enum 타입을 사용하여 변수 타입을 정의
    /// </summary>
    public enum CardVariableKey
    {
        None,
        
        // 필요한 변수 타입을 여기에 추가.
        
    }
    [Serializable]
    public class VariableContainer : VariableContainer<CardVariableKey>
    {
    }
/// <summary>
/// 변수 컨테이너.
/// 
/// </summary>
[Serializable]
public class VariableContainer<TEnum> : ICopy<VariableContainer<TEnum>> where TEnum : Enum
{
    [SerializeField]private List<KeyVariablePair<TEnum>> variables = new ();
    public List<KeyVariablePair<TEnum>> Variables => variables;
    public void Clear() => variables.Clear();
    


    
    [Serializable]
    public class KeyVariablePair<TEnum> : ICloneable where TEnum : Enum
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
            return new KeyVariablePair<TEnum>
            {
                key = key,
                stringValue = stringValue,
                intValue = intValue,
                floatValue = floatValue
            };
        }
    }
    
    public bool HasVariable(TEnum variableKey) => HasVariable(variableKey.ToString());
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
    
    public KeyVariablePair<TEnum> GetVariable(TEnum variableKey)
    {
        return GetVariable(variableKey.ToString());
    }
    public KeyVariablePair<TEnum> GetVariable(string key)
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
    
    public bool TryGetVariable(TEnum variableKey, out KeyVariablePair<TEnum> variablePair)
    {
        return TryGetVariable(variableKey.ToString(), out variablePair);
    }
    public bool TryGetVariable(string key, out KeyVariablePair<TEnum> variablePair)
    {
        variablePair = GetVariable(key);
        return variablePair != null;
    }
    public bool TryGetString(TEnum variableKey, out string value)
    {
        return TryGetString(variableKey.ToString(), out value);
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
    public bool TryGetInt(TEnum variableKey, out int value)
    {
        return TryGetInt(variableKey.ToString(), out value);
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
    public bool TryGetFloat(TEnum variableKey, out float value)
    {
        return TryGetFloat(variableKey.ToString(), out value);
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
    
    
    public string GetString(TEnum variableKey) => GetStringDefault(variableKey.ToString(), string.Empty);
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
    
    public int GetInt(TEnum variableKey) => GetIntDefault(variableKey.ToString(), 0);
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
    
    public void SetString(TEnum variableKey, string value)
    {
        SetString(variableKey.ToString(), value);
    }
    public void SetString(string key, string value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair<TEnum>();
            variable.key = key;
            variables.Add(variable);
        }
        variable.stringValue = value;
    }
    public void SetInt(TEnum variableKey, int value)
    {
        SetInt(variableKey.ToString(), value);
    }
    public void SetInt(string key, int value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair<TEnum>();
            variable.key = key;
            variables.Add(variable);
        }
        variable.intValue = value;
    }
    
    public void AddInt(TEnum variableKey, int value)
    {
        AddInt(variableKey.ToString(), value);
    }
    public void AddInt(string key, int value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair<TEnum>();
            variable.key = key;
            variables.Add(variable);
        }
        variable.intValue += value;
    }
    
    public void SetFloat(TEnum variableKey, float value)
    {
        SetFloat(variableKey.ToString(), value);
    }
    public void SetFloat(string key, float value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair<TEnum>();
            variable.key = key;
            variables.Add(variable);
        }
        variable.floatValue = value;
    }
    public float GetFloat(TEnum variableKey) => GetFloatDefault(variableKey.ToString(), 0f);
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
    
    public void AddFloat(TEnum variableKey, float value)
    {
        AddFloat(variableKey.ToString(), value);
    }
    public void AddFloat(string key, float value)
    {
        var variable = GetVariable(key);
        if (variable == null)
        {
            variable = new KeyVariablePair<TEnum>();
            variable.key = key;
            variables.Add(variable);
        }
        variable.floatValue += value;
    }
    
    
    public void CopyFrom(VariableContainer<TEnum> target)
    {
        Clear();
        foreach (var argument in target.variables)
        {
            variables.Add((KeyVariablePair<TEnum>)argument.Clone());
        }
    }
    
    public void CopyTo(VariableContainer<TEnum> target)
    {
        target.CopyFrom(this);
    }
    
    public VariableContainer<TEnum> Clone()
    {
        var clone = new VariableContainer<TEnum>();
        clone.CopyFrom(this);
        return clone;
    }
}

}