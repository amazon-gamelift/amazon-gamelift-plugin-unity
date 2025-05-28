// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Collections;
using System.Globalization;
using AmazonGameLiftPlugin.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SampleTests.UI
{
    public sealed class SignUpTests
    {
        private TestSettings _settings;
        private AwsTestIdentity _testIdentity;
        private GameLiftClientSettingsOverride _settingsOverride;

        [SetUp]
        public void SetUp()
        {
            _settings = new TestSettingsSource().Get();
            _testIdentity = new AwsTestIdentity(_settings);
            _testIdentity.CreateTestUser();
            _settingsOverride = new GameLiftClientSettingsOverride();
            _settingsOverride.SetUp(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            _settingsOverride?.TearDown();
            _testIdentity.DeleteTestUser();
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenNoCredentialsEnteredAndSubmitted_IsActive()
        {
            yield return GoToSignUpScreen();

            GameObject screen = null;

            while (screen == null)
            {
                screen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            SubmitAndAssertNoError(screen);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenNoEmailEnteredAndSubmitted_IsActive()
        {
            yield return GoToSignUpScreen();

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var passwordInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, _testIdentity.Password);

            SubmitAndAssertNoError(signUpScreen);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenInvalidEmailEnteredAndSubmitted_IsActiveAndLogsError()
        {
            const string testEmail = "invalid";
            const string expectedError = "Username should be an email.";
            _testIdentity.SignUpTestUser();
            yield return GoToSignUpScreen();

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var emailInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, testEmail);

            var passwordInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, _testIdentity.Password);

            yield return SubmitAndAssertError(signUpScreen, expectedError);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenNoPasswordEnteredAndSubmitted_IsActive()
        {
            yield return GoToSignUpScreen();

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var emailInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, _testIdentity.Email);

            SubmitAndAssertNoError(signUpScreen);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenShortPasswordEnteredAndSubmitted_IsActive()
        {
            const string testPassword = "1234567";
            _testIdentity.DeleteTestUser();
            yield return GoToSignUpScreen();

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var emailInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, _testIdentity.Email);

            var passwordInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, testPassword);

            SubmitAndAssertNoError(signUpScreen);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenInvalidPasswordEnteredAndSubmitted_IsActiveAndLogsError()
        {
            const string testPassword = "12345678";
            const string expectedError = "Password did not conform with policy: Password must have lowercase characters";
            _testIdentity.DeleteTestUser();
            yield return GoToSignUpScreen();

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var emailInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, _testIdentity.Email);

            var passwordInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, testPassword);

            yield return SubmitAndAssertError(signUpScreen, expectedError);
        }

        [UnityTest]
        public IEnumerator SignUpScreen_WhenValidCredentialsEnteredAndSubmitted_ShowsSignInScreen()
        {
            string testCode = Random.Range(100_000, 1_000_000)
                .ToString(CultureInfo.InvariantCulture);

            PlayModeUtility.DestroyAll<GameLift>();

            _testIdentity.DeleteTestUser();
            _testIdentity.CreateTestUser();
            yield return GoToSignUpScreen();

            GameLift gameLift = Object.FindObjectOfType<GameLift>();
            Assert.IsNotNull(gameLift);

            var testConfiguration = new GameLiftConfiguration
            {
                AwsRegion = _settings.Region,
                UserPoolClientId = _settings.UserPoolClientId,
            };
            var coreApi = new SignUpTestGameLiftCoreApi(_settings, testConfiguration);
            var client = new GameLiftClient(coreApi, new Delay(), Logger.SharedInstance);
            gameLift.OverrideClient(client);

            GameObject signUpScreen = null;

            while (signUpScreen == null)
            {
                signUpScreen = GameObject.Find(GameObjectNames.SignUpScreen);
                yield return null;
            }

            var emailInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/EmailInputField");
            yield return TestInput.EnterText(emailInputField, _testIdentity.Email);

            var passwordInputField = GameObject.Find(GameObjectNames.SignUpScreen + "/PasswordInputField");
            yield return TestInput.EnterText(passwordInputField, _testIdentity.Password);

            var submitButton = GameObject.Find(GameObjectNames.SignUpScreen + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            var waitForConfirmation = new WaitForGameObjectFound(GameObjectNames.ConfirmationCodeScreen, timeoutSec: 10f);
            yield return waitForConfirmation;

            Assert.IsFalse(waitForConfirmation.TimedOut, $"Object {GameObjectNames.ConfirmationCodeScreen} was never loaded");

            var confirmationScreen = GameObject.Find(GameObjectNames.ConfirmationCodeScreen);
            Assert.IsFalse(signUpScreen.activeSelf);
            Assert.IsTrue(confirmationScreen.activeSelf);

            var codeInputField = GameObject.Find(GameObjectNames.ConfirmationCodeScreen + "/CodeInputField");
            yield return TestInput.EnterText(codeInputField, testCode);

            submitButton = GameObject.Find(GameObjectNames.ConfirmationCodeScreen + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            var waitForSignIn = new WaitForGameObjectFound(GameObjectNames.SignInScreen, timeoutSec: 1f);
            yield return waitForSignIn;

            Assert.IsFalse(waitForSignIn.TimedOut, $"Object {GameObjectNames.SignInScreen} was never loaded");

            var signInScreen = GameObject.Find(GameObjectNames.SignInScreen);
            Assert.IsTrue(signInScreen.activeSelf);
            Assert.IsFalse(signUpScreen.activeSelf);
            Assert.IsFalse(confirmationScreen.activeSelf);
        }

        private IEnumerator GoToSignUpScreen()
        {
            SceneManager.LoadScene("BootstrapScene");
            const string screenName = GameObjectNames.SignInScreen;
            GameObject signInScreen = null;

            while (signInScreen == null)
            {
                signInScreen = GameObject.Find(screenName);
                yield return null;
            }

            var signUpButton = GameObject.Find(screenName + "/SignUpButton");
            yield return TestInput.PressButton(signUpButton);
        }

        private IEnumerator SubmitAndAssertNoError(GameObject screen)
        {
            var submitButton = GameObject.Find(screen.name + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            var message = GameObject.Find(screen.name + "/MessageText");
            string messageText = TestInput.GetText(message);

            Assert.IsTrue(screen.activeSelf);
            Assert.IsTrue(string.IsNullOrEmpty(messageText));
        }

        private IEnumerator SubmitAndAssertError(GameObject screen, string expectedError)
        {
            LogAssert.Expect(LogType.Error, "Sign-up error AWSERROR: " + expectedError);

            var submitButton = GameObject.Find(screen.name + "/SubmitButton");
            yield return TestInput.PressButton(submitButton);

            var message = GameObject.Find(screen.name + "/MessageText");
            string messageText = TestInput.GetText(message);

            Assert.IsTrue(screen.activeSelf);
            Assert.AreEqual(expectedError, messageText);
        }
    }
}
