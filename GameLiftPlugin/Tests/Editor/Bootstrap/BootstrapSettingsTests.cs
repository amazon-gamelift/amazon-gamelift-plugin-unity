// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using AmazonGameLiftPlugin.Core.BucketManagement.Models;
using AmazonGameLiftPlugin.Core.SettingsManagement.Models;
using AmazonGameLiftPlugin.Core.Shared;
using Moq;
using NUnit.Framework;
using UnityEditor;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public class BootstrapSettingsTests
    {
        private readonly BucketUrlFormatter _bucketUrlFormatter = new BucketUrlFormatter();
        private static Mock<CoreApi> _coreApiMock;
        private static Mock<StateManager> _stateManager;

        const string TestBucketName = "test-bucket";
        const string TestRegion = "test-region";
        const string TestProfile = "test-profile";


        [SetUp]
        public void Setup()
        {
            _coreApiMock = new Mock<CoreApi>();
            _stateManager = new Mock<StateManager>(_coreApiMock.Object);
            SetupHappyPath();
        }

        #region CreateBucket

        [Test]
        public void CreateBucket_WhenCanCreateIsFalse_NotCalling()
        {
            _coreApiMock.Verify(
                target => target.CreateBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
            _coreApiMock.Verify(
                target => target.PutBucketLifecycleConfiguration(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<BucketPolicy>()), Times.Never());

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(_coreApiMock.Object);
            bootstrapUtilityMock.Verify(target => target.GetBootstrapData(), Times.Never());

            BootstrapSettings underTest = GetUnitUnderTest(bootstrapUtility: bootstrapUtilityMock);

            // Act
            underTest.CreateBucket(null);

            // Assert
            _coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
        }

        [Test]
        public void CreateBucket_WhenCanCreateAndBootstrapDataNotFound_IsStatusError()
        {
            const string testBucketName = "test-bucket";

            var currentResponse = new GetSettingResponse();
            currentResponse = Response.Fail(currentResponse);
            _coreApiMock.Setup(target => target.GetSetting(It.IsAny<SettingsKeys>()))
                .Returns(currentResponse)
                .Verifiable();

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(_coreApiMock.Object);

            GetBootstrapDataResponse bootstrapResponse = Response.Fail(new GetBootstrapDataResponse());
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);

            // Act
            underTest.CreateBucket(testBucketName);

            // Assert
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Error, underTest.Status.Type);
        }

        [Test]
        public void CreateBucket_WhenCanCreateAndBootstrapDataFoundAndCreationFailed_IsStatusError()
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";
            const string testBucketName = "test-bucket";

            var createResponse = new CreateBucketResponse();
            createResponse = Response.Fail(createResponse);
            _coreApiMock.Setup(target => target.CreateBucket(testProfile, testRegion, It.IsAny<string>()))
                .Returns(createResponse)
                .Verifiable();

            var bootstrapUtilityMock = new Mock<BootstrapUtility>(_coreApiMock.Object);

            var bootstrapResponse = new GetBootstrapDataResponse(testProfile, testRegion);
            bootstrapResponse = Response.Ok(bootstrapResponse);
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);

            // Act
            underTest.CreateBucket(testBucketName);

            // Assert
            _coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Error, underTest.Status.Type);
        }

        [Test]
        public void CreateBucket_WhenCreationSuccessAndHasNoPolicy_IsStatusInfo()
        {
            var bootstrapUtilityMock = new Mock<BootstrapUtility>(_coreApiMock.Object);

            _coreApiMock.Verify(
                target => target.PutBucketLifecycleConfiguration(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<BucketPolicy>()), Times.Never());

            BootstrapSettings underTest = SetUpCreateBucketSuccess(bootstrapUtilityMock: bootstrapUtilityMock);

            // Act
            underTest.LifeCyclePolicyIndex = 0;
            underTest.CreateBucket("bucket-name");

            // Assert
            _coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Info, underTest.Status.Type);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreateBucket_WhenCreationSuccessAndHasPolicyAndPutPolicySuccessParam_IsStatusInfo(
            bool isPutPolicySuccess)
        {
            const string testProfile = "test-profile";
            const string testRegion = "test-region";
            const string testBucketName = "test-bucket";
            var bootstrapUtilityMock = new Mock<BootstrapUtility>(_coreApiMock.Object);

            var putPolicyResponse = new PutLifecycleConfigurationResponse();
            putPolicyResponse = isPutPolicySuccess
                ? Response.Ok(putPolicyResponse)
                : Response.Fail(putPolicyResponse);
            _coreApiMock.Setup(target =>
                    target.PutBucketLifecycleConfiguration(testProfile, testRegion, testBucketName,
                        It.IsAny<BucketPolicy>()))
                .Returns(putPolicyResponse)
                .Verifiable();

            BootstrapSettings underTest =
                SetUpCreateBucketSuccess(testProfile, testRegion, testBucketName, bootstrapUtilityMock);

            // Act
            underTest.LifeCyclePolicyIndex = 1;
            underTest.CreateBucket(testBucketName);

            // Assert
            _coreApiMock.Verify();
            bootstrapUtilityMock.Verify();
            Assert.AreEqual(MessageType.Info, underTest.Status.Type);
        }

        private static BootstrapSettings SetUpCreateBucketSuccess(
            string testProfile = "test-profile", string testRegion = "test-region",
            string testBucketName = "test-bucket",
            Mock<BootstrapUtility> bootstrapUtilityMock = null)
        {
            var createResponse = new CreateBucketResponse();
            createResponse = Response.Ok(createResponse);
            _coreApiMock.Setup(target => target.CreateBucket(testProfile, testRegion, It.IsAny<string>()))
                .Returns(createResponse)
                .Verifiable();

            bootstrapUtilityMock = bootstrapUtilityMock ?? new Mock<BootstrapUtility>(_coreApiMock.Object);
            var bootstrapResponse = new GetBootstrapDataResponse(testProfile, testRegion);
            bootstrapResponse = Response.Ok(bootstrapResponse);
            bootstrapUtilityMock.Setup(target => target.GetBootstrapData())
                .Returns(bootstrapResponse)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(bootstrapUtility: bootstrapUtilityMock);
            underTest.SelectBucket(testBucketName);
            return underTest;
        }

        #endregion

        #region Current Bucket

        #region HasCurrentBucket

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketNotSavedAndRegionSaved_IsFalse()
        {
            _stateManager.Object.BucketName = null;

            _coreApiMock.Setup(target => target.IsValidRegion(TestRegion))
                .Returns(true);

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketSavedAndRegionNotSaved_IsFalse()
        {
            _coreApiMock.SetUpCoreApiWithBucket(success: true);
            _coreApiMock.SetUpCoreApiWithRegion(success: false, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketSavedAndRegionSavedInvalid_IsFalse()
        {
            _coreApiMock.SetUpCoreApiWithBucket(success: true);
            _coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            AssertNoCurrentBucket(underTest);
        }

        [Test]
        public void HasCurrentBucket_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsTrue()
        {
            _coreApiMock.SetUpCoreApiForBootstrapSuccess();
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            Assert.IsTrue(underTest.HasCurrentBucket);
        }

        #endregion

        [Test]
        public void CurrentBucketName_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testBucketName = "test-bucket";

            _coreApiMock.SetUpCoreApiForBootstrapSuccess(testBucketName);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            Assert.AreEqual(testBucketName, underTest.CurrentBucketName);
        }

        [Test]
        public void CurrentBucketUrl_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testBucketName = "test-bucket";
            const string testRegion = "test-region";

            _coreApiMock.SetUpCoreApiForBootstrapSuccess(testBucketName, testRegion);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            string expectedUrl = _bucketUrlFormatter.Format(testBucketName, testRegion);
            Assert.AreEqual(expectedUrl, underTest.CurrentBucketUrl);
        }

        [Test]
        public void CurrentRegion_WhenRefreshCurrentBucketAndBucketAndRegionSaved_IsExpected()
        {
            const string testRegion = "test-region";

            _coreApiMock.SetUpCoreApiForBootstrapSuccess(testRegion: testRegion);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();

            // Assert
            _coreApiMock.Verify();
            Assert.AreEqual(testRegion, underTest.CurrentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshCurrentBucketAndBucketSavedAndRegionSavedInvalid_IsNull()
        {
            _coreApiMock.SetUpCoreApiWithBucket(success: true);
            _coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshCurrentBucket();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        private static void AssertNoCurrentBucket(BootstrapSettings underTest)
        {
            Assert.IsFalse(underTest.HasCurrentBucket);
            Assert.IsNull(underTest.CurrentBucketName);
            Assert.IsNull(underTest.CurrentBucketUrl);
        }

        #endregion

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionNotSaved_IsNull()
        {
            _coreApiMock.Setup(target => target.IsValidRegion(It.IsAny<string>()))
                .Returns(true);

            _stateManager.Object.Region = null;

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionSavedInvalid_IsNull()
        {
            _coreApiMock.SetUpCoreApiWithBucket(success: true);
            _stateManager.Object.Region = null;
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(currentRegion);
        }

        [Test]
        public void CurrentRegion_WhenRefreshBucketNameAndRegionAndAccountIdAndProfileSaved_IsExpected()
        {
            const string testRegion = "test-region";

            _coreApiMock.SetUpCoreApiWithRegion(success: true, valid: true, testRegion: testRegion);
            _coreApiMock.SetUpCoreApiWithAccountId(success: true);
            _coreApiMock.SetUpCoreApiWithProfile(success: true);

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string currentRegion = underTest.CurrentRegion;

            // Assert
            _coreApiMock.Verify();
            Assert.AreEqual(testRegion, currentRegion);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionNotSaved_IsNull()
        {
            _coreApiMock.SetUpCoreApiWithRegion(success: false, valid: false);

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionSavedAndInvalid_IsNull()
        {
            _coreApiMock.SetUpCoreApiWithBucket(success: true);
            _coreApiMock.SetUpCoreApiWithRegion(success: true, valid: false);
            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndCurrentProfileInvalid_IsNull()
        {
            _coreApiMock.SetUpCoreApiForBootstrapSuccess();
            _coreApiMock.SetUpCoreApiWithProfile(success: false);

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndAccountIdInvalid_IsNull()
        {
            _coreApiMock.SetUpCoreApiForBootstrapSuccess();
            _coreApiMock.SetUpCoreApiWithProfile(success: true);
            _coreApiMock.SetUpCoreApiWithAccountId(success: false);

            BootstrapSettings underTest = GetUnitUnderTest();

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            _coreApiMock.Verify();
            Assert.IsNull(name);
        }

        [Test]
        public void BucketName_WhenRefreshBucketNameAndRegionAndAccountIdAndProfileSaved_IsExpected()
        {
            var bucketNameFormatterMock = new Mock<IBucketNameFormatter>();
            bucketNameFormatterMock.Setup(target => target.FormatBucketName(It.IsAny<string>(), TestRegion))
                .Returns(TestBucketName)
                .Verifiable();

            BootstrapSettings underTest = GetUnitUnderTest(bucketNameFormatterMock);

            // Act
            underTest.RefreshBucketName();
            string name = underTest.BucketName;

            // Assert
            _coreApiMock.Verify();
            Assert.AreEqual(TestBucketName, name);
        }

        [Test]
        public void BucketName_WhenNewInstance_IsNull()
        {
            var bucketNameFormatterMock = new Mock<IBucketNameFormatter>();
            BootstrapSettings underTest = GetUnitUnderTest(bucketNameFormatterMock);
            Assert.IsNull(underTest.BucketName);
        }

        private static BootstrapSettings GetUnitUnderTest(Mock<IBucketNameFormatter> bucketNameFormatterMock = null,
            Mock<BootstrapUtility> bootstrapUtility = null)
        {
            bucketNameFormatterMock = bucketNameFormatterMock ?? new Mock<IBucketNameFormatter>();
            bootstrapUtility = bootstrapUtility ?? new Mock<BootstrapUtility>(_coreApiMock.Object);

            TextProvider textProvider = TextProviderFactory.Create();
            string[] policyNames = new string[] { "none", "30 days" };
            var lifecyclePolicies = new BucketPolicy[] { BucketPolicy.None, BucketPolicy.ThirtyDaysLifecycle };
            return new BootstrapSettings(lifecyclePolicies, policyNames, textProvider, bucketNameFormatterMock.Object,
                new MockLogger(), _stateManager.Object, _coreApiMock.Object, bootstrapUtility.Object);
        }

        private void SetupHappyPath()
        {
            _coreApiMock.SetUpCoreApiWithAccountId(success: true);
            _coreApiMock.SetUpCoreApiWithProfile(success: true);
            _coreApiMock.Setup(f => f.GetSetting(It.IsAny<SettingsKeys>()))
                .Returns(Response.Ok(new GetSettingResponse()));
            _coreApiMock.Setup(f => f.PutSetting(It.IsAny<SettingsKeys>(), It.IsAny<string>()))
                .Returns(Response.Ok(new PutSettingResponse()));
            _coreApiMock.Setup(f => f.IsValidRegion(It.IsAny<string>())).Returns(true);

            _stateManager = new Mock<StateManager>(_coreApiMock.Object)
            {
                Object =
                {
                    Region = TestRegion,
                    BucketName = TestBucketName
                }
            };
            _stateManager.SetupGet(l => l.ProfileName).Returns(TestProfile);
        }
    }
}