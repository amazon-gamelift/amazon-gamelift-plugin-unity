// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AmazonGameLiftPlugin.Core.Latency.Models;

namespace AmazonGameLiftPlugin.Core.Latency
{
    public class PingWrapper : IPingWrapper
    {
        // A small, non-empty datagram. Amazon GameLift Servers UDP ping beacons echo
        // the payload back; the body content is not significant.
        private static readonly byte[] PingPayload = { 0x50, 0x49, 0x4e, 0x47 }; // "PING"

        private readonly int _timeoutMs;
        private const int DefaultTimeoutMs = 2000;

        public PingWrapper(int timeoutMs = DefaultTimeoutMs)
        {
            _timeoutMs = timeoutMs <= 0 ? DefaultTimeoutMs : timeoutMs;
        }

        // Measures round-trip latency to a UDP ping beacon by sending a datagram to the
        // beacon (gamelift-ping.<region>.api.aws:7770) and timing the echoed response.
        public async Task<PingResult> SendPingAsync(string host, int port)
        {
            try
            {
                // Resolve the beacon host asynchronously so the DNS lookup runs off the
                // calling thread and regions resolve concurrently.
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
                IPAddress address = Array.Find(addresses, a => a.AddressFamily == AddressFamily.InterNetwork)
                                    ?? addresses[0];

                using (var udpClient = new UdpClient(address.AddressFamily))
                using (var timeoutCancellation = new CancellationTokenSource())
                {
                    udpClient.Connect(new IPEndPoint(address, port));

                    var stopwatch = Stopwatch.StartNew();

                    await udpClient.SendAsync(PingPayload, PingPayload.Length);

                    Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
                    Task timeoutTask = Task.Delay(_timeoutMs, timeoutCancellation.Token);

                    Task completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                    if (completedTask != receiveTask)
                    {
                        // No response within the timeout: count this as a lost sample.
                        // Observe the abandoned receive's fault (the socket is disposed on
                        // exit) so it does not raise an unobserved task exception.
                        _ = receiveTask.ContinueWith(
                            t => { _ = t.Exception; },
                            TaskContinuationOptions.OnlyOnFaulted);
                        return new PingResult { Success = false };
                    }

                    // Response arrived; cancel the pending timeout timer.
                    timeoutCancellation.Cancel();

                    // Observe the receive result so a faulted socket surfaces as an exception.
                    await receiveTask;

                    stopwatch.Stop();

                    return new PingResult
                    {
                        RoundtripTime = stopwatch.ElapsedMilliseconds,
                        Success = true
                    };
                }
            }
            catch (SocketException)
            {
                // Treat any socket-level failure (DNS resolution error, or an ICMP
                // "port unreachable" surfacing as ConnectionReset on Windows) as a lost sample.
                return new PingResult { Success = false };
            }
        }
    }
}
