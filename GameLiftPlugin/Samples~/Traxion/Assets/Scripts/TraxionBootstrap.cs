// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

#if UNITY_SERVER

using UnityEngine;

/// <summary>
/// Server entry-point MonoBehaviour — placed on a persistent GameObject in the
/// server scene.  It ensures GameLift is initialised before
/// <see cref="TraxionManager"/> calls into it.
/// </summary>
[DefaultExecutionOrder(-100)]  // run before TraxionManager
public class TraxionBootstrap : MonoBehaviour
{
    [SerializeField] private TraxionGameLiftServer _gameLiftServer;

    private void Awake()
    {
        if (_gameLiftServer == null)
            _gameLiftServer = GetComponentInChildren<TraxionGameLiftServer>(true);

        if (_gameLiftServer != null)
            _gameLiftServer.Initialise();

        Debug.Log("[Traxion Bootstrap] Server bootstrap complete");
    }
}

#endif // UNITY_SERVER
