using System;
using SharpPcap;

public delegate void ActionFrameArrived(PacketDotNet.Ieee80211.ActionFrame a);
public interface IBroadcastCapturer
{
    string Filter { get; set; }
    void StartCapture();
    event ActionFrameArrived OnActionFrameArrived;
}
