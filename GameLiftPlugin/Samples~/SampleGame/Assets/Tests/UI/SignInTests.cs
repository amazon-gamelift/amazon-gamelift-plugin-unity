// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SampleTests.UI
{
    public sealed class SignInTests
    {
        private const string NextSceneName = "GameScene";

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
        public IEnumerator SignInScreen_WhenNoCredentialsEnteredAndSubmitted_IsActive()
        {
            SceneManager.LoadScene("BootstrapScene");
            const string screenName = GameObjectNames.SignInScreen;
            GameObject signInScreen = null;

            while (signInScreen == null)
            {
                signInScreen = GameObject.Find(screenName);
                yield return null;
            }

            var submitButton = GameObject.Find(screenName + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            Assert.IsTrue(signInScreen.activeSelf);
            var message = GameObject.Find(screenName + "/MessageText");
            string messageText = TestInput.GetText(message);
            Assert.IsTrue(string.IsNullOrEmpty(messageText));
        }

        [UnityTest]
        public IEnumerator SignInScreen_WhenNoEmailEnteredAndSubmitted_IsActive()
        {
            SceneManager.LoadScene("BootstrapScene");
            const string screenName = GameObjectNames.SignInScreen;
            GameObject signInScreen = null;

            while (signInScreen == null)
            {
                signInScreen = GameObject.Find(screenName);
                yield return null;
            }

            var passwordInputField = GameObject.Find(screenName + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, "0000");

            var submitButton = GameObject.Find(screenName + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            Assert.IsTrue(signInScreen.activeSelf);
            var message = GameObject.Find(screenName + "/MessageText");
            string messageText = TestInput.GetText(message);
            Assert.IsTrue(string.IsNullOrEmpty(messageText));
        }

        [UnityTest]
        public IEnumerator SignInScreen_WhenInvalidCredentialsEnteredAndSubmitted_IsActiveAndLogsError()
        {
            SceneManager.LoadScene("BootstrapScene");
            const string screenName = GameObjectNames.SignInScreen;
            GameObject signInScreen = null;

            while (signInScreen == null)
            {
                signInScreen = GameObject.Find(screenName);
                yield return null;
            }

            var emailInputField = GameObject.Find(screenName + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, "invalid@mail");

            var passwordInputField = GameObject.Find(screenName + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, "0000");

            LogAssert.Expect(LogType.Error, "Sign-in error AWSERROR: Incorrect username or password.");

            var submitButton = GameObject.Find(screenName + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            Assert.IsTrue(signInScreen.activeSelf);
            var message = GameObject.Find(screenName + "/MessageText");
            string messageText = TestInput.GetText(message);
            Assert.AreEqual("Incorrect username or password.", messageText);
        }

        [UnityTest]
        public IEnumerator SignInScreen_WhenValidCredentialsEnteredAndSubmitted_LoadsGame()
        {
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

            var waitForScene = new WaitForSceneLoaded(NextSceneName, timeoutSec: 5);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, $"Scene {NextSceneName} was never loaded");
        }
    }
}
