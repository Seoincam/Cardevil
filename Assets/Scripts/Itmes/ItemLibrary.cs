using Cardevil.DebugConsole;
using Cardevil.Pools;
using Cardevil.Utils;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using Console = Cardevil.DebugConsole.Console;
using MessageType = Cardevil.DebugConsole.MessageType;

namespace Cardevil.Item
{
    public class ItemLibrary
    {
        private static ItemLibrary _instance;
        
        private Dictionary<string, ICloneFactory<Item>> _itemDictionary;
        
        private class ItemFactory : ICloneFactory<Item>
        {
            public Item Original { get; set; }

            public ItemFactory(Item original)
            {
                Original = original;
            }
            
            public Item Create()
            {
                return Original.DeepClone();
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new ItemLibrary();
            }
            
            /*
             *  아이템 등록
             */

            void DefaultRegisterAll()
            {
                // 자동 등록
                var itemTypes = ReflectionUtil.FindDerivedTypes<Item>();
                var registerDef = typeof(ItemLibrary)
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .First(m => m.Name == "Register"
                                && m.IsGenericMethodDefinition
                                && m.GetGenericArguments().Length == 1
                                && m.GetParameters().Length == 0);
                foreach (var type in itemTypes)
                {
                    if (type.IsAbstract) continue; 
                    try
                    {
                        var genericMethod = registerDef.MakeGenericMethod(type);
                        genericMethod.Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to register item type {type.Name}: {ex.Message}");
                    }
                    
                }
            }
            
            // {클래스이름: 클래스} 형태로 기본 등록
            DefaultRegisterAll();
        }
        
        
        [MenuItem("Cardevil/Item Library/Print All Items")]
        public static void PrintAllItems()
        {
            if (_instance == null)
            {
                LogEx.LogError("ItemLibrary is not initialized.");
                return;
            }
            
            foreach (var item in _instance._itemDictionary)
            {
                LogEx.Log($"Item Name: {item.Key}, Item Type: {item.Value.Original.GetType().Name}");
            }
        }
        [Preserve, ConsoleCommand("printItems", "Print all registered items in the ItemLibrary.")]
        public static void PrintAllItemsCommand()
        {
            if (_instance == null)
            {
                Console.MessageError("ItemLibrary is not initialized.");
                return;
            }
            
            foreach (var item in _instance._itemDictionary)
            {
                Console.MessageDefault($"Item Name: {item.Key}, Item Type: {item.Value.Original.GetType().Name}");
            }
        }
        
        public static Item GetItem(string name)
        {
            if (_instance == null)
            {
                LogEx.LogError("ItemLibrary is not initialized.");
                return null;
            }
            
            if (_instance._itemDictionary != null && _instance._itemDictionary.TryGetValue(name, out var factory))
            {
                return factory.Create();
            }
            LogEx.LogWarning($"Item with name {name} not found.");
            return null;
        }
        
        public static bool Contains(string name)
        {
            if (_instance == null || _instance._itemDictionary == null)
            {
                return false;
            }
            return _instance._itemDictionary.ContainsKey(name);
        }

        public static void Register<T>() where T : Item, new()
        {
            if (_instance == null)
            {
                _instance = new ItemLibrary();
            }
            string name = typeof(T).Name;
            _instance.RegisterInternal<T>(name);
        }
        
        public static void Register<T>(string name) where T : Item, new()
        {
            if (_instance == null)
            {
                _instance = new ItemLibrary();
            }
            _instance.RegisterInternal<T>(name);
        }
        
        public static void Register(string name, Item original)
        {
            if (_instance == null)
            {
                _instance = new ItemLibrary();
            }
            _instance.RegisterInternal(name, original);
        }
        
        public static void Register(string name, ICloneFactory<Item> factory)
        {
            if (_instance == null)
            {
                _instance = new ItemLibrary();
            }
            _instance.RegisterInternal(name, factory);
        }
        
        public static void Unregister(string name)
        {
            if (_instance != null && _instance._itemDictionary != null)
            {
                if (_instance._itemDictionary.Remove(name))
                {
                    LogEx.Log($"Item with name {name} unregistered.");
                }
                else
                {
                    LogEx.LogWarning($"Item with name {name} not found to unregister.");
                }
            }
        }
        
        public static void Clear()
        {
            if (_instance != null && _instance._itemDictionary != null)
            {
                _instance._itemDictionary.Clear();
            }
        }
        
        public static IEnumerable<string> GetAllItemNames()
        {
            if (_instance != null && _instance._itemDictionary != null)
            {
                return _instance._itemDictionary.Keys;
            }
            return new List<string>();
        }
        
        public static IEnumerable<Item> GetAllItems()
        {
            if (_instance != null && _instance._itemDictionary != null)
            {
                foreach (var factory in _instance._itemDictionary.Values)
                {
                    yield return factory.Create();
                }
            }
        }
        
        private void RegisterInternal<T>(string name) where T : Item, new()
        {
            T original = new T();

            original.itemName = name;
            
            RegisterInternal(name, new ItemFactory(original));
        }
        
        private void RegisterInternal(string name, Item original)
        {
            if (original == null)
            {
                Debug.LogError("Original item is null.");
                return;
            }
            RegisterInternal(name, new ItemFactory(original));
            
        }
        
        private void RegisterInternal(string name, ICloneFactory<Item> factory)
        {
            LogEx.Log($"Registering item: {name}");
            if (_itemDictionary == null)
            {
                _itemDictionary = new ();
            }
            
            if (factory?.Original == null)
            {
                LogEx.LogError("Factory or its Original is null.");
                return;
            }
            
            if (_itemDictionary.ContainsKey(name))
            {
                LogEx.LogWarning($"Item with name {name} is already registered. Overwriting.");
            }
            _itemDictionary[name] = factory;
        }
    }
}