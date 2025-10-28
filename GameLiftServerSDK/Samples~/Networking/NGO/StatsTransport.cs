using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

#if UNITY_SERVER || UNITY_EDITOR
using Aws.GameLift.Unity.Metrics;
#endif

/// <summary>
/// Minimal decorator transport that wraps an inner NetworkTransport (e.g., UnityTransport)
/// to accumulate total bytes/packets in and out. Assign this as the NetworkManager's
/// transport and set InnerTransport to your real transport.
/// </summary>
public class StatsTransport : NetworkTransport
{
    // The real transport to forward calls to (e.g., UnityTransport).
    public NetworkTransport InnerTransport;

    // Totals (kept serialized so you can observe in the Inspector in Play Mode)
    [SerializeField] private long _totalBytesIn;
    [SerializeField] private long _totalBytesOut;
    [SerializeField] private long _totalPacketsIn;
    [SerializeField] private long _totalPacketsOut;

    public long TotalBytesIn => _totalBytesIn;
    public long TotalBytesOut => _totalBytesOut;
    public long TotalPacketsIn => _totalPacketsIn;
    public long TotalPacketsOut => _totalPacketsOut;

    // UnityTransport reference for connection data access
    private NetworkTransport IT => InnerTransport ?? throw new InvalidOperationException("StatsTransport: InnerTransport is not assigned. Ensure a valid transport component is configured.");
    private UnityTransport UnityTransportInstance => IT as UnityTransport ?? throw new InvalidOperationException("StatsTransport: InnerTransport must be UnityTransport for this operation.");

    private void Awake()
    {
        // If InnerTransport isn't set explicitly, try to find any other NetworkTransport on the same GameObject.
        if (InnerTransport == null)
        {
            var transports = GetComponents<NetworkTransport>();
            foreach (var transport in transports)
            {
                if (transport != this && transport.GetType() != typeof(StatsTransport))
                {
                    InnerTransport = transport;
                    break;
                }
            }
        }

        if (InnerTransport == null)
        {
            Debug.LogError("StatsTransport is missing an InnerTransport (e.g., UnityTransport) on the same GameObject.");
        }
    }

    private void CountSend(int bytes)
    {
        if (bytes <= 0) return;

        _totalBytesOut += bytes;
        _totalPacketsOut += 1;

#if UNITY_SERVER || UNITY_EDITOR
        if (GameLiftMetrics.IsInitialized && GameLiftMetrics.Network != null)
        {
            GameLiftMetrics.Network.IncrementBytesOut(bytes);
            GameLiftMetrics.Network.IncrementPacketsOut(1);
        }
#endif
    }

    private void CountReceive(int bytes)
    {
        if (bytes > 0)
        {
            _totalBytesIn += bytes;
            _totalPacketsIn += 1;

#if UNITY_SERVER || UNITY_EDITOR
            if (GameLiftMetrics.IsInitialized && GameLiftMetrics.Network != null)
            {
                GameLiftMetrics.Network.IncrementBytesIn(bytes);
                GameLiftMetrics.Network.IncrementPacketsIn(1);
            }
#endif
        }
    }

    // --- NetworkTransport overrides (forwarding to InnerTransport) --- //

    // Essential properties that must be forwarded
    public override ulong ServerClientId => IT.ServerClientId;

    // Forward IsSupported property
    public override bool IsSupported => IT.IsSupported;

    public override void Initialize(NetworkManager networkManager)
    {
        IT.OnTransportEvent += OnInnerTransportEvent;
        IT.Initialize(networkManager);
    }

    private void OnInnerTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        // Forward transport events to NetworkManager
        InvokeOnTransportEvent(eventType, clientId, payload, receiveTime);

        // Count data for stats (use event-based counting to avoid double counting with PollEvent)
        if (eventType == NetworkEvent.Data)
        {
            CountReceive(payload.Count);
        }
    }
    public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
    {
        IT.Send(clientId, payload, networkDelivery);
        CountSend(payload.Count);
    }

    public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
    {
        return IT.PollEvent(out clientId, out payload, out receiveTime);
    }

    public override bool StartClient()
    {
        return IT.StartClient();
    }

    public override bool StartServer()
    {
        return IT.StartServer();
    }

    public override void Shutdown()
    {
        // Unsubscribe from events before shutdown
        IT.OnTransportEvent -= OnInnerTransportEvent;
        IT.Shutdown();
    }

    public override void DisconnectLocalClient()
    {
        IT.DisconnectLocalClient();
    }

    public override void DisconnectRemoteClient(ulong clientId)
    {
        IT.DisconnectRemoteClient(clientId);
    }

    public override ulong GetCurrentRtt(ulong clientId)
    {
        return IT.GetCurrentRtt(clientId);
    }

    // Expose UnityTransport connection data access methods
    public void SetConnectionData(string address, ushort port, string listenAddress = null)
    {
        UnityTransportInstance.ConnectionData.Address = address;
        UnityTransportInstance.ConnectionData.Port = port;
        if (!string.IsNullOrEmpty(listenAddress))
        {
            UnityTransportInstance.ConnectionData.ServerListenAddress = listenAddress;
        }
    }

    public UnityTransport.ConnectionAddressData GetConnectionData()
    {
        return UnityTransportInstance.ConnectionData;
    }

    // Clean up event subscriptions
    private void OnDestroy()
    {
        if (InnerTransport != null)
        {
            InnerTransport.OnTransportEvent -= OnInnerTransportEvent;
        }
    }
}
