// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AmazonGameLiftPlugin.Editor.UnitTests
{
    public static class AssertAsync
    {
        public delegate Task AsyncTestDelegate();

        // https://forum.unity.com/threads/can-i-replace-upgrade-unitys-nunit.488580/
        public static TActual ThrowsAsync<TActual>(AsyncTestDelegate code, string message = "", params object[] args) where TActual : Exception
        {
            return Assert.Throws<TActual>(() =>
            {
                try
                {
                    code.Invoke().Wait(); // Will wrap any exceptions in an AggregateException
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is null)
                    {
                        throw;
                    }

                    throw e.InnerException; // Throw the unwrapped exception
                }
            }, message, args);
        }
    }
}
