// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using AmazonGameLift.Editor;
using NUnit.Framework;
using UnityEditor;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class StatusTests
    {
        [Test]
        public void IsDisplayed_WhenNewInstance_IsFalse()
        {
            var underTest = new Status();
            Assert.IsFalse(underTest.IsDisplayed);
        }

        [Test]
        public void GetMessage_WhenNewInstance_IsNull()
        {
            var underTest = new Status();
            Assert.IsNull(underTest.Message);
        }

        [Test]
        public void GetMessage_WhenNewInstanceAndMessageSet_ReturnsMessage()
        {
            string testMessage = DateTime.Now.ToString();
            var underTest = new Status();

            underTest.SetMessage(testMessage, MessageType.Info);

            Assert.AreEqual(testMessage, underTest.Message);
        }

        [Test]
        public void OnChangedEvent_WhenNewInstanceAndSetMessage_IsRaised()
        {
            string testMessage = DateTime.Now.ToString();
            var underTest = new Status();

            bool isEventRaised = false;
            underTest.Changed += () =>
            {
                isEventRaised = true;
            };

            // Act
            underTest.SetMessage(testMessage, MessageType.Info);

            // Assert
            Assert.IsTrue(isEventRaised);
        }

        [Test]
        public void OnChangedEvent_WhenSetMessageSame_IsNotRaised()
        {
            string testMessage = DateTime.Now.ToString();
            string testMessage2 = testMessage;
            MessageType testType = MessageType.None;
            var underTest = new Status();
            underTest.SetMessage(testMessage, testType);

            bool isEventRaised = false;
            underTest.Changed += () =>
            {
                isEventRaised = true;
            };

            // Act
            underTest.SetMessage(testMessage2, testType);

            // Assert
            Assert.IsFalse(isEventRaised);
        }

        [Test]
        public void OnChangedEvent_WhenSetMessageDifferent_IsRaised()
        {
            string testMessage = DateTime.Now.ToString();
            string testMessage2 = testMessage + "2";
            MessageType testType = MessageType.None;
            var underTest = new Status();
            underTest.SetMessage(testMessage, testType);

            bool isEventRaised = false;
            underTest.Changed += () =>
            {
                isEventRaised = true;
            };

            // Act
            underTest.SetMessage(testMessage2, testType);

            // Assert
            Assert.IsTrue(isEventRaised);
        }

        [Test]
        public void OnChangedEvent_WhenSetMessageSameAndDifferentType_IsRaised()
        {
            string testMessage = DateTime.Now.ToString();
            MessageType testType = MessageType.None;
            MessageType testType2 = MessageType.Info;
            var underTest = new Status();
            underTest.SetMessage(testMessage, testType);

            bool isEventRaised = false;
            underTest.Changed += () =>
            {
                isEventRaised = true;
            };

            // Act
            underTest.SetMessage(testMessage, testType2);

            // Assert
            Assert.IsTrue(isEventRaised);
        }
    }
}
