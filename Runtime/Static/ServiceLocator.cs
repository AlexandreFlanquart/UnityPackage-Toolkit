﻿using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public static class ServiceLocator
    {
        //Hold Reference to all found services as type as key , and reference to the concerete object
    static Dictionary<object, object> servicecontainer = null;

    /// <summary>
    /// Find a service/script in current scene and return reference of it , Note: it will still find the service even if it's unactive
    /// </summary>
    /// <typeparam name="T">Type of service to find</typeparam>
    /// <returns></returns>
    public static T GetService<T>(bool createObjectIfNotFound = false) where T : Object
    {
        //Init the dictionary
        if (servicecontainer == null)
            servicecontainer = new Dictionary<object, object>();

        try
        {
            //Check if the key exist in the dictionary
            if (servicecontainer.ContainsKey(typeof(T)))
            {
                T service = (T)servicecontainer[typeof(T)];
                if (service != null) //If Key exist and the object it reference to still exist
                {
                    return service;
                }
                else //The key exist but reference object doesn't exist anymore
                {
                    servicecontainer.Remove(typeof(T)); //Remove this key from the dictonary
                    return FindService<T>(createObjectIfNotFound); //Try and find the service in current scene
                }
            }
            else
            {
                return FindService<T>(createObjectIfNotFound);
            }
        }
        catch
        {
            throw new System.NotImplementedException("Can't find requested service, and create new one is set to " + createObjectIfNotFound);
        }
    }


    /// <summary>
    /// Add a service/script
    /// </summary>
    /// <typeparam name="T">Type of service to add</typeparam>
    /// <param name="go">GameObject of service to add</param>
    public static void AddService<T>(GameObject go) where T : Object
    {
        //Init the dictionary
        if (servicecontainer == null)
            servicecontainer = new Dictionary<object, object>();

        try
        {
            //Check if the key exist in the dictionary
            if (servicecontainer.ContainsKey(typeof(T)))
            {
                T service = (T)servicecontainer[typeof(T)];
                if (service == null) //The key exist but reference object doesn't exist anymore
                {
                    servicecontainer.Remove(typeof(T)); //Remove this key from the dictonary
                    servicecontainer.Add(typeof(T), go.GetComponent<T>());
                }
            }
            else
            {
                servicecontainer.Add(typeof(T), go.GetComponent<T>());
            }
        }
        catch
        {
            throw new System.NotImplementedException("The requested service is already referenced");
        }
    }

    /// <summary>
    /// Check if a service is already referenced
    /// </summary>
    /// <returns>If a service is already referenced</returns>
    public static bool Exists<T>() where T : Object
    {
        //Init the dictionary
        if (servicecontainer == null)
            servicecontainer = new Dictionary<object, object>();

        try
        {
            //Check if the key exist in the dictionary
            if (servicecontainer.ContainsKey(typeof(T)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            throw new System.NotImplementedException("An error has occured");
        }
    }

    /// <summary>
    /// Look for a game object with type required
    /// </summary>
    /// <typeparam name="T">Type to look for</typeparam>
    /// <param name="createObjectIfNotFound">Either create a gameobject with type if not exist</param>
    /// <returns></returns>
    static T FindService<T>(bool createObjectIfNotFound = false) where T : Object
    {
        T type = GameObject.FindAnyObjectByType<T>();
        if (type != null)
        {
            //If found add it to the dictonary
            servicecontainer.Add(typeof(T), type);
        }
        else if (createObjectIfNotFound)
        {
            //If not found and set to create new gameobject , create a new gameobject and add the type to it
            GameObject go = new GameObject(typeof(T).Name, typeof(T));
            servicecontainer.Add(typeof(T), go.GetComponent<T>());
        }
        return (T)servicecontainer[typeof(T)];
    }

    public static void Clear()
    {
        servicecontainer.Clear();
    }
    }
}