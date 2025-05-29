// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SampleTests.UI
{
    public sealed class WaitForSceneLoaded : WaitWithTimeOut
    {
        private readonly string _sceneName;

        public override bool keepWaiting
        {
            get
            {
                Scene scene = SceneManager.GetSceneByName(_sceneName);
                bool sceneLoaded = scene.IsValid() && scene.isLoaded;
                return !sceneLoaded && base.keepWaiting;
            }
        }

        public WaitForSceneLoaded(string sceneName, float timeoutSec = 10)
            : base(timeoutSec, () => Time.realtimeSinceStartup)
        {
            _sceneName = sceneName;
        }
    }
}
