using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

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
        musicEventInstance = RuntimeManager.CreateInstance(musicEvent);
        musicEventInstance.start();
        ambEventInstance = RuntimeManager.CreateInstance(ambEvent);
        ambEventInstance.start();
    }

    //--------------------------------------------------------------------
    // Method to stop the music event, allowing the
    // ADSR envelope fadeout settings from FMOD
    //--------------------------------------------------------------------
    public void StopMusic()
    {
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicEventInstance.release();
        ambEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambEventInstance.release();
    }

    //--------------------------------------------------------------------
    // Method for setting a parameter
    //--------------------------------------------------------------------
    public void SetMusicParameter(string parameterName, float value)
    {
        musicEventInstance.setParameterByName(parameterName, value);
    }

    //--------------------------------------------------------------------
    // Method for playing a one-shot sound
    //--------------------------------------------------------------------
    public void PlayOneShot(EventReference sound, Vector3 position)
    {
        RuntimeManager.PlayOneShot(sound, position);
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