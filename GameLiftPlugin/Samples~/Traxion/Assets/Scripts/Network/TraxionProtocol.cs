// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Low-level framing protocol: 4-byte big-endian length prefix + UTF-8 body.
/// Identical to the SampleGame <c>NetworkProtocol</c> so both can share a
/// server fleet if desired.
/// </summary>
public static class TraxionProtocol
{
    public static void Send(TcpClient client, string message)
    {
        if (client == null) return;
        NetworkStream stream    = client.GetStream();
        byte[]        payload   = Encoding.UTF8.GetBytes(message);
        byte[]        lenBytes  = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
        stream.Write(lenBytes, 0, lenBytes.Length);
        stream.Write(payload,  0, payload.Length);
    }

    public static string[] Receive(TcpClient client)
    {
        NetworkStream   stream   = client.GetStream();
        var             messages = new List<string>();

        while (stream.DataAvailable)
        {
            byte[] lenBuf = new byte[4];
            stream.Read(lenBuf, 0, 4);
            int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));

            byte[] body = new byte[size];
            stream.Read(body, 0, size);
            messages.Add(Encoding.UTF8.GetString(body));
        }

        return messages.ToArray();
    }
}

/// <summary>
/// Connection handshake payload — sent by the client immediately after TCP connect.
/// </summary>
[Serializable]
public class TraxionConnectionInfo
{
    public string ipAddress;
    public int    port;
    public string playerSessionId;
    public string playerName;

    public static TraxionConnectionInfo FromJson(string json) =>
        UnityEngine.JsonUtility.FromJson<TraxionConnectionInfo>(json);
}
