using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Assertions.Must;

public class AudioManager : MonoBehaviour
{
    //--------------------------------------------------------------------
    // Creates a place in the UI to select your FMod event file
    //--------------------------------------------------------------------
    public EventReference musicEvent;
    public EventReference ambEvent;

    //--------------------------------------------------------------------
    // Setting up the instance of the event
    // (in Start we will put the Event we selected above into this variable)
    //--------------------------------------------------------------------
    private EventInstance musicEventInstance;
    private EventInstance ambEventInstance;

    //--------------------------------------------------------------------
    // Setup this audio manager and make sure there is only one of it
    //--------------------------------------------------------------------
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //--------------------------------------------------------------------
    // On game start, create an instance of the music event and play it
    //--------------------------------------------------------------------
    private void Start()
    {
        StartAmbience();
    }

    public void StartMusic()
    {
        if (!musicEventInstance.isValid())
        {
            musicEventInstance = RuntimeManager.CreateInstance(musicEvent);
            musicEventInstance.start();
            musicEventInstance.release();
        }
    }

    public void StopMusic()
    {
        StopEvent(ref musicEventInstance);
    }

    private void StartAmbience()
    {
        if (!ambEventInstance.isValid())
        {
            ambEventInstance = RuntimeManager.CreateInstance(ambEvent);
            ambEventInstance.start();
            musicEventInstance.release();
        }
    }

    //--------------------------------------------------------------------
    // Method for setting a parameter
    //--------------------------------------------------------------------
    public void SetMusicParameter(string parameterName, float value)
    {
        musicEventInstance.setParameterByName(parameterName, value);
    }

    public static bool StartEvent(
        EventReference eventRef,
        GameObject self,
        out EventInstance eventInst,
        params (string name, float value)[] parameters
    )
    {
        if (eventRef.IsNull)
        {
            Debug.LogWarning($"StartEvent: event is null");
            eventInst = default;
            return false;
        }

        try
        {
            eventInst = RuntimeManager.CreateInstance(eventRef);
        }
        catch (System.Exception)
        {
            eventInst = default;
        }

        if (!eventInst.isValid())
        {
            Debug.LogWarning($"StartEvent: failed to start event");
            eventInst = default;
            return false;
        }

        if (self != null)
            RuntimeManager.AttachInstanceToGameObject(eventInst, self);

        foreach (var (name, value) in parameters)
        {
            eventInst.setParameterByName(name, value);
        }

        eventInst.start();
        eventInst.release();
        return true;
    }

    public static bool StartEvent(
        EventReference eventRef,
        Component self,
        out EventInstance eventInst,
        params (string name, float value)[] parameters
    )
    {
        return StartEvent(eventRef, self.gameObject, out eventInst, parameters);
    }

    public static bool StartEvent(EventReference eventRef, out EventInstance eventInst, params (string name, float value)[] parameters)
    {
        return StartEvent(eventRef, null as GameObject, out eventInst, parameters);
    }

    public static bool StopEvent(ref EventInstance eventInst)
    {
        if (!eventInst.isValid())
            return false;

        eventInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        eventInst.clearHandle();
        return true;
    }
}

/*
//--------------------------------------------------------------------
//  Play the sfx event one-shot at the game object's current location
//--------------------------------------------------------------------
AudioManager.Instance.PlayOneShot(FMODEvents.Instance.exampleSound, exampleObject.transform.position);

//----------------------------------------------------------------------
// Change the FMOD parameters of the music
//----------------------------------------------------------------------
AudioManager.Instance.SetMusicParameter("parameterName", parameterValue);

//--------------------------------------------------------------------
//  Stop the game music
//--------------------------------------------------------------------
AudioManager.Instance.StopMusic();
 */
