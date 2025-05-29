// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public static class NetworkProtocol
{
    public static string[] Receive(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        var messages = new List<string>();

        while (stream.DataAvailable)
        {
            byte[] bufferLength = new byte[4];
            stream.Read(bufferLength, 0, bufferLength.Length);
            int msgSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bufferLength, 0));
            byte[] readBuffer = new byte[msgSize];
            stream.Read(readBuffer, 0, readBuffer.Length);
            string msgStr = Encoding.ASCII.GetString(readBuffer, 0, readBuffer.Length);
            messages.Add(msgStr);
        }

        return messages.ToArray();
    }

    public static void Send(TcpClient client, string msgStr)
    {
        if (client == null)
        {
            return;
        }

        NetworkStream stream = client.GetStream();
        byte[] writeBuffer = Encoding.ASCII.GetBytes(msgStr);
        int msgSize = writeBuffer.Length;
        byte[] bufferLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgSize));
        stream.Write(bufferLength, 0, bufferLength.Length);
        stream.Write(writeBuffer, 0, msgSize);
    }
}
