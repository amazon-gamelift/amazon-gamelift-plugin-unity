// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    /*
     * StatefulInput represents a section of UI that holds inputs and/or can be affected by
     * changes to other UI input. It is required that StatefulInput's implement UpdateGUI
     * which can be called when the input or it's container's state changes.
     */
    public abstract class StatefulInput
    {
        private const string InactiveFoldoutElementName = "hidden";

        //Should be called with a visual element to hide this element on the screen
        public static void Hide(VisualElement element) => element?.AddToClassList(InactiveFoldoutElementName);
        //Should be called with a visual element to display this item on the screen
        public static void Show(VisualElement element) => element?.RemoveFromClassList(InactiveFoldoutElementName);
        //Should be called with a visual element to display or hide the element on the screen
        public static void ShowHide(VisualElement element, bool showElement)
        {
            if (showElement)
            {
                Show(element);
            }
            else
            {
                Hide(element);
            }
        }

        //Should be called after a change in the UI state to update the UI to reflect the change
        protected abstract void UpdateGUI();
    }
}