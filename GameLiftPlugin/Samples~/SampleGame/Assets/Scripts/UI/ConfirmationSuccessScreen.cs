// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

public sealed class ConfirmationSuccessScreen : FormScreen<ConfirmationSuccessScreen.SubmitDelegate>
{
    public delegate void SubmitDelegate();

    internal override bool CheckCanSubmit()
    {
        return true;
    }

    protected override void Submit(SubmitDelegate onSubmit)
    {
        onSubmit();
    }
}
