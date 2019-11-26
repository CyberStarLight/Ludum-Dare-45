using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SmartBeheviour : MonoBehaviour
{
    public static Dictionary<string, HashSet<SmartBeheviour>> FindByLayer = new Dictionary<string, HashSet<SmartBeheviour>>();
    public static Dictionary<string, HashSet<SmartBeheviour>> FindByTag = new Dictionary<string, HashSet<SmartBeheviour>>();
    public static Dictionary<Type, Dictionary<SmartBeheviour, ComponentInfo>> FindByComponent = new Dictionary<Type, Dictionary<SmartBeheviour, ComponentInfo>>();
    public static Dictionary<string, SmartBeheviour> FindByUniqueName = new Dictionary<string, SmartBeheviour>();
    public static Dictionary<int, SmartBeheviour> FindByUniqueId = new Dictionary<int, SmartBeheviour>();
    public static Dictionary<int, SmartBeheviour> FindByInstanceId = new Dictionary<int, SmartBeheviour>();

    public int UniqueId { get; private set; }

    [Header("Smart Beheviour")]
    public string UniqueName;
    public string[] Tags;

    //private variables
    private static int NextUniqueId = int.MinValue;

    private int _registeredUniqueId;
    private string _registeredUniqueName;
    private string _registeredLayer;
    private HashSet<string> _registeredTags = new HashSet<string>();
    private HashSet<Type> _registeredCompnents = new HashSet<Type>();
    
    //Events
    public virtual void Awake()
    {
        //Get new unique Id
        UniqueId = NextUniqueId;
        NextUniqueId++;

        FindByUniqueId[UniqueId] = this;
        _registeredUniqueId = UniqueId;

        //register instance ID
        FindByInstanceId[gameObject.GetInstanceID()] = this;

        //Register unique Name
        if (!String.IsNullOrEmpty(UniqueName))
        {
            FindByUniqueName[UniqueName] = this;
            _registeredUniqueName = UniqueName;
        }

        //Register tags
        if (!string.IsNullOrEmpty(gameObject.tag))
        {
            RegisterTag(gameObject.tag, this);
            _registeredTags.Add(gameObject.tag);
        }

        foreach (var tag in Tags)
        {
            RegisterTag(tag, this);
            _registeredTags.Add(tag);
        }

        //Register layer
        string layerName = LayerMask.LayerToName(gameObject.layer);
        RegisterLayer(layerName, this);
        _registeredLayer = layerName;
    }

    public virtual void OnDestroy()
    {
        //Unregister tags
        foreach (var tag in _registeredTags)
            UnregisterTag(tag, this);

        //Unregister components
        foreach (var componentType in _registeredCompnents)
            UnregisterComponent(this, componentType);

        //unegister layer
        if(_registeredLayer != null)
            UnregisterLayer(_registeredLayer, this);

        //unegister unique name
        if (_registeredUniqueName != null)
            FindByUniqueName.Remove(_registeredUniqueName);

        //unegister unique id
        FindByUniqueId.Remove(_registeredUniqueId);

        //unregister instance id
        FindByInstanceId.Remove(GetInstanceID());
    }

    //public methods
    public bool HasTag(string tag)
    {
        //return false if there is no such tag list yet
        HashSet<SmartBeheviour> existingSet;
        if (!FindByTag.TryGetValue(tag, out existingSet))
            return false;
        
        //check if this object is registered in the tag list
        return existingSet.Contains(this);
    }

    //Private methods
    public T GetRegisteredComponent<T>() where T : Component
    {
        return GetRegisteredComponent<T>(this);
    }

    public static T ComponentFromInstanceID<T>(Transform obj) where T : Component
    {
        return ComponentFromInstanceID<T>(obj.gameObject);
    }

    public static T ComponentFromInstanceID<T>(GameObject obj) where T : Component
    {
        if (FindByInstanceId.TryGetValue(obj.GetInstanceID(), out SmartBeheviour target))
        {
            //target is a smartbeheviour, get already registered component
            return target.GetRegisteredComponent<T>();
        }
        else
        {
            //target is not a smartbeheviour, get component using the normal way
            return obj.GetComponent<T>();
        }
    }

    //Private static methods
    protected static void RegisterComponent<T>(SmartBeheviour obj, T component) where T : Component
    {
        Type type = typeof(T);
        //Get the current list or create a new one if it doesn't exist yet
        Dictionary<SmartBeheviour, ComponentInfo> existingSet;
        if (!FindByComponent.TryGetValue(type, out existingSet))
        {
            existingSet = new Dictionary<SmartBeheviour, ComponentInfo>();
            FindByComponent[type] = existingSet;
        }

        existingSet.Add(obj, new ComponentInfo(type, component));
        obj._registeredCompnents.Add(type);
    }

    protected static void UnregisterComponent(SmartBeheviour obj, Type componentType)
    {
        //Get the current list or create a new one if it doesn't exist yet
        Dictionary<SmartBeheviour, ComponentInfo> existingSet;
        if (!FindByComponent.TryGetValue(componentType, out existingSet))
            return;

        existingSet.Remove(obj);
    }

    protected static T GetRegisteredComponent<T>(SmartBeheviour obj) where T : Component
    {
        Type type = typeof(T);

        //Get the current list or create a new one if it doesn't exist yet
        Dictionary<SmartBeheviour, ComponentInfo> existingSet;
        if (!FindByComponent.TryGetValue(type, out existingSet))
        {
            existingSet = new Dictionary<SmartBeheviour, ComponentInfo>();
            FindByComponent[type] = existingSet;
        }

        ComponentInfo componentInfo;
        if (!existingSet.TryGetValue(obj, out componentInfo))
        {
            T component = obj.GetComponent<T>();
            existingSet.Add(obj, new ComponentInfo(type, component));
            
            obj._registeredCompnents.Add(type);

            return component;
        }
        else
        {
            return componentInfo.HasComponent ? (T)componentInfo.Component : null;
        }
    }
    
    private static void RegisterTag(string tag, SmartBeheviour obj)
    {
        //Get the current list or create a new one if it doesn't exist yet
        HashSet<SmartBeheviour> existingSet;
        if(!FindByTag.TryGetValue(tag, out existingSet))
        {
            existingSet = new HashSet<SmartBeheviour>();
            FindByTag[tag] = existingSet;
        }

        existingSet.Add(obj);

    }

    private static void RegisterLayer(string layerName, SmartBeheviour obj)
    {
        //Get the current list or create a new one if it doesn't exist yet
        HashSet<SmartBeheviour> existingSet;
        if (!FindByLayer.TryGetValue(layerName, out existingSet))
        {
            existingSet = new HashSet<SmartBeheviour>();
            FindByLayer[layerName] = existingSet;
        }

        existingSet.Add(obj);
    }

    private static void UnregisterTag(string tag, SmartBeheviour obj)
    {
        //Get the current list or ignore action if it doesn't exist yet
        HashSet<SmartBeheviour> existingSet;
        if (!FindByTag.TryGetValue(tag, out existingSet))
            return;

        existingSet.Remove(obj);
    }

    private static void UnregisterLayer(string layerName, SmartBeheviour obj)
    {
        //Get the current list or ignore action if it doesn't exist yet
        HashSet<SmartBeheviour> existingSet;
        if (!FindByLayer.TryGetValue(layerName, out existingSet))
            return;

        existingSet.Remove(obj);
    }
}

public struct ComponentInfo
{
    public Type ComponentType;
    public Component Component;
    public bool HasComponent;

    public ComponentInfo(Type _ComponentType, Component _Component)
    {
        ComponentType = _ComponentType;
        Component = _Component;
        HasComponent = (Component != null);
    }
}

public static class SmartBeheviourExtentions
{
    public static SmartBeheviour GetSmartBeheviour(this MonoBehaviour obj)
    {
        SmartBeheviour result = null;
        SmartBeheviour.FindByInstanceId.TryGetValue(obj.GetInstanceID(), out result);

        return result;
    }
}