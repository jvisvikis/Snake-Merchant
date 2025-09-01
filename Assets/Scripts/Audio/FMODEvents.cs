using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODEvents : MonoBehaviour
{
    // -----------------------------------------------------------------
    // This creates the UI to select/drag in the FMOD file paths
    // for all your sounds. Just copy and paste this line and
    // rename for as many sounds as you need.
    // -----------------------------------------------------------------
    [field: SerializeField] public EventReference exampleSound { get; private set; }

    // -----------------------------------------------------------------
    // This sets up the FMODEvents manager and ensures there is only one
    // -----------------------------------------------------------------
    public static FMODEvents Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }

        Instance = this;
    }
}