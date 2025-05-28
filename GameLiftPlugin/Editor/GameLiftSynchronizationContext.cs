// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#nullable enable

using System;
using UnityEngine;
using System.Threading;
using AmazonGameLift.Editor;

/**
 * Wrapper of 'System.Threading.SynchronizationContext' that provides additional functionality such as
 * debugging support.
 */
public class GameLiftSynchronizationContext
{
    /**
     * Underlying context to access methods that have no other utility
     */
    public SynchronizationContext Context { get; private set; }

    internal GameLiftSynchronizationContext(SynchronizationContext context)
    {
        Context = context;
    }

    /**
     * Starts a synchronous request to send a message.
     * Logs exceptions that occur to the Unity console and optionally trigger a callback.
     * For example, the callback can be used to display the exception in the UI.
     */
    public void Send(SendOrPostCallback d, object? state, Action<Exception>? onException = null)
    {
        Context.Send(WrapWithExceptionHandling(d, onException), state);
    }

    /**
     * Starts an asynchronous request to post a message.
     * Logs exceptions that occur to the Unity console and optionally trigger a callback.
     */
    public void Post(SendOrPostCallback d, object? state, Action<Exception>? onException = null)
    {
        Context.Post(WrapWithExceptionHandling(d, onException), state);
    }

    /**
     * Starts an asynchronous request to log a message to the Unity console.
     */
    public void Log(string errorMessage)
    {
        Post(_ => UnityEngine.Debug.Log(errorMessage), null);
    }

    /**
     * Starts an asynchronous request to log an error to the Unity console.
     */
    public void LogError(string errorMessage)
    {
        Post(_ => UnityEngine.Debug.LogError(errorMessage), null);
    }

    /**
     * Starts an asynchronous request to log an exception to the Unity console.
     */
    public void LogException(Exception exception)
    {
        Post(_ => UnityEngine.Debug.LogException(exception), null);
    }

    /**
     * Wraps a SendOrPostCallback with an exception handler.
     * Exceptions are logged to the Unity console and optionally trigger a callback.
     */
    private SendOrPostCallback WrapWithExceptionHandling(SendOrPostCallback callback, Action<Exception>? onException = null)
    {
        return state =>
        {
            try
            {
                callback.Invoke(state);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Unexpected exception in async context: {e}");
                SafelyInvoke(onException, e, "failure callback");
            }
        };
    }

    /**
     * Attempts to invoke provided callback. If the callback is null, this is a no-op.
     * If the callback fails, will log the exception to the Unity console.
     */
    private void SafelyInvoke(Action<Exception>? callback, Exception exception, string operation = "callback")
    {
        try
        {
            callback?.Invoke(exception);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Unexpected exception in async context invoking {operation}: {e}");
        }
    }

    /**
     * Gets the synchronization context for the current thread.
     */
    public static GameLiftSynchronizationContext Current => new GameLiftSynchronizationContext(System.Threading.SynchronizationContext.Current);
}
