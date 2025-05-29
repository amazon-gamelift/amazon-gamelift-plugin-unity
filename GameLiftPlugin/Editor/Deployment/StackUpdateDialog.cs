// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.DeploymentManagement.Models;
using UnityEditor;
using UnityEngine;

namespace AmazonGameLift.Editor
{
    internal sealed class StackUpdateDialog : Dialog
    {
        private const float ScrollViewHeight = 100f;
        private const float VerticalSpacingPixels = 5f;

        private HyperLinkButton _helpLinkButton;
        private Vector2 _scrollPosition;
        private StackUpdateModel _model;
        private string[] _changes;
        private string[] _changeCount;
        private string _labelQuestion;
        private string _labelRemovalHeader;
        private string _labelConsoleWarning;

        protected override bool IsModal { get; } = true;

        protected override float WindowWidthPixels { get; } = 450f;

        protected override string TitleKey => Strings.TitleStackUpdateDialog;

        public Task<bool> SetUp(StackUpdateModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _changes = model.RemovalChanges
                .Select(FormatChange)
                .ToArray();
            _changeCount = model.ChangesByAction
                .Select(FormatChangeCount)
                .ToArray();
            _helpLinkButton.Url = _model.CloudFormationUrl;
            var completionSource = new TaskCompletionSource<bool>();
            AddAction(new DialogAction(Strings.LabelStackUpdateCancelButton, () => completionSource.TrySetResult(false)));
            AddAction(new DialogAction(Strings.LabelStackUpdateConfirmButton, () => completionSource.TrySetResult(true)));
            DefaultAction = () => completionSource.TrySetResult(false);
            return completionSource.Task;
        }

        private string FormatChangeCount(KeyValuePair<string, Change[]> group)
        {
            string format = TextProvider.Get(Strings.LabelStackUpdateCountTemplate);
            return string.Format(format, group.Key, group.Value.Count());
        }

        protected override void SetUpContents()
        {
            titleContent = new GUIContent(TextProvider.Get(Strings.TitleStackUpdateDialog));

            _helpLinkButton = new HyperLinkButton(TextProvider.Get(Strings.LabelStackUpdateConsole),
              "", ResourceUtility.GetHyperLinkStyle());

            _labelQuestion = TextProvider.Get(Strings.LabelStackUpdateQuestion);
            _labelRemovalHeader = TextProvider.Get(Strings.LabelStackUpdateRemovalHeader);
            _labelConsoleWarning = TextProvider.Get(Strings.LabelStackUpdateConsoleWarning);
        }

        protected override Rect DrawContents()
        {
            if (_model == null)
            {
                return new Rect();
            }

            using (var mainScope = new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label(_labelQuestion);

                if (_model.HasRemovalChanges)
                {
                    GUILayout.Space(2 * VerticalSpacingPixels);
                    GUILayout.Label(_labelRemovalHeader);
                    GUILayout.Space(2 * VerticalSpacingPixels);

                    using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.Height(ScrollViewHeight)))
                    {
                        _scrollPosition = scope.scrollPosition;

                        foreach (string item in _changes)
                        {
                            GUILayout.Label(item);
                        }
                    }
                }

                GUILayout.Space(3 * VerticalSpacingPixels);

                foreach (string item in _changeCount)
                {
                    GUILayout.Label(item);
                }

                GUILayout.Space(3 * VerticalSpacingPixels);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5f);
                    _helpLinkButton.Draw();
                }

                GUILayout.Space(VerticalSpacingPixels);
                EditorGUILayout.HelpBox(_labelConsoleWarning, MessageType.Warning);
                GUILayout.Space(VerticalSpacingPixels);
                return mainScope.rect;
            }
        }

        private static string FormatChange(Change change)
        {
            return $"{change.LogicalId} ({change.ResourceType})";
        }
    }
}
