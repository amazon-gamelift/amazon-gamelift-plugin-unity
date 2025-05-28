// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using UnityEngine;

[System.Serializable]
public class ConnectionInfo
{
    [SerializeField]
    private string _ipAddress;
    [SerializeField]
    private int _port;
    [SerializeField]
    private string _playerSessionId;

    public string IpAddress
    {
        get { return _ipAddress; }
        set { _ipAddress = value; }
    }
    public int Port
    {
        get { return _port; }
        set { _port = value; }
    }
    public string PlayerSessionId
    {
        get { return _playerSessionId; }
        set { _playerSessionId = value; }
    }

    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public static ConnectionInfo CreateFromSerial(string json)
    {
        var temp = new ConnectionInfo();
        temp.Deserialize(json);
        return temp;
    }

    public override string ToString()
    {
        return string.Format("ConnectionInfo (IpAddress: {0}, Port: {1}, PlayerSessionId: {2})", IpAddress, Port, PlayerSessionId); 
    }

    private void Deserialize(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
