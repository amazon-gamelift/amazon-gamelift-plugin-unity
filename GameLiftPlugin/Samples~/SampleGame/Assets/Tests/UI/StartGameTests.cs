// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SampleTests.UI
{
    public sealed class StartGameTests
    {
        private const string GameSceneName = "GameScene";
        private AwsTestIdentity _testIdentity;
        private GameLiftClientSettingsOverride _settingsOverride;

        [SetUp]
        public void SetUp()
        {
            TestSettings settings = new TestSettingsSource().Get();
            _testIdentity = new AwsTestIdentity(settings);
            _testIdentity.SignUpTestUser();
            _settingsOverride = new GameLiftClientSettingsOverride();
            _settingsOverride.SetUp(settings);
        }

        [TearDown]
        public void TearDown()
        {
            _settingsOverride?.TearDown();
            _testIdentity.DeleteTestUser();
        }

        [UnityTest]
        public IEnumerator StartGame_WhenValidCredentialsEnteredAndStartGameSubmitted_LoadsGame()
        {
            PlayModeUtility.DestroyAll<GameLift>();
            SceneManager.LoadScene("BootstrapScene");
            const string screenName = GameObjectNames.SignInScreen;
            GameObject signInScreen = null;

            while (signInScreen == null)
            {
                signInScreen = GameObject.Find(screenName);
                yield return null;
            }

            var emailInputField = GameObject.Find(screenName + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, _testIdentity.Email);

            var passwordInputField = GameObject.Find(screenName + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, _testIdentity.Password);

            var submitButton = GameObject.Find(screenName + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            var waitForScene = new WaitForSceneLoaded(GameSceneName, timeoutSec: 5);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, $"Scene {GameSceneName} was never loaded");

            const string screenName2 = GameObjectNames.StartGameScreen;
            var startScreen = GameObject.Find(screenName2);

            Assert.IsTrue(startScreen.activeSelf);

            submitButton = GameObject.Find(screenName2 + "/SubmitButton");
            Assert.IsNotNull(submitButton);
            Assert.IsTrue(submitButton.activeSelf);

            Button buttonScript = submitButton.GetComponent<Button>();
            Assert.IsNotNull(buttonScript);
            Assert.IsTrue(buttonScript.interactable);

            yield return TestInput.PressButton(submitButton);

            var messageText = GameObject.Find(screenName2 + "/MessageText");
            Assert.IsNotNull(messageText);

            string message = TestInput.GetText(messageText);
            Assert.AreEqual(string.Empty, message);

            CanvasGroup screenGroup = startScreen.GetComponent<CanvasGroup>();
            Assert.IsNotNull(screenGroup);
            Assert.IsFalse(screenGroup.interactable);

            var waitForMessage = new WaitUnitilCondition(() => TestInput.GetText(messageText) != message || !startScreen.activeSelf, timeoutSec: 20f);
            yield return waitForMessage;

            Assert.IsFalse(waitForMessage.TimedOut, $"The session status was never updated");
        }
    }
}
