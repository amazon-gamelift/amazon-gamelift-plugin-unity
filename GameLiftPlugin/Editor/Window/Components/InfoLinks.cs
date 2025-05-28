// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class InfoLinks : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InfoLinks> { }
        
        public InfoLinks()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/InfoLinks");
            asset.CloneTree(this);

            LocalizeText();

            this.Q<Label>("InfoLinkDocumentationLink")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsHelpGameLiftUnityDocumentation));
            this.Q<Label>("InfoLinkForumLink").RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.AwsGameTechForums));
            this.Q<Label>("InfoLinkReportIssuesLink")
                .RegisterCallback<ClickEvent>(_ => Application.OpenURL(Urls.GitHubAwsIssues));
        }

        private void LocalizeText()
        {
            var l = new ElementLocalizer(this);
            l.SetElementText("InfoLinkDocumentationLink", Strings.InfoLinkDocumentationLink);
            l.SetElementText("InfoLinkForumLink", Strings.InfoLinkForumLink);
            l.SetElementText("InfoLinkReportIssuesLink", Strings.InfoLinkReportIssuesLink);
        }
    }
}