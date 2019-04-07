using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpPcap;
using SharpPcap.LibPcap;

public class DiscoveryMonitor : IMonitor
{
    private readonly Settings settings;
    private readonly ILogger<DiscoveryMonitor> logger;
    private readonly IBroadcastCapturer broadcastCapturer;

    public DiscoveryMonitor(Settings settings, ILogger<DiscoveryMonitor> logger, IBroadcastCapturer broadcastCapturer)
    {
        this.settings = settings;
        this.logger = logger;
        this.broadcastCapturer = broadcastCapturer;
    }

    public Task Initialize()
    {
        this.logger.LogInformation("Initializing Discovery Monitor");

        this.broadcastCapturer.Filter = "broadcast";
        this.broadcastCapturer.OnActionFrameArrived += this.OnPacketArrival;

        return Task.CompletedTask;
    }

    public Task Listen()
    {
        this.broadcastCapturer.StartCapture();

        return Task.CompletedTask;
    }

    private void OnPacketArrival(PacketDotNet.Ieee80211.ActionFrame actionFrame)
    {
        if (BufferMatchers.FindPatternEndsWith(actionFrame.PayloadData, this.settings.TogglePacket))
        {
            var addressBytes = actionFrame.SourceAddress.GetAddressBytes();

            var addressString = BitConverter
                                    .ToString(addressBytes)
                                    .Replace("-", ":")
                                    .ToLower();

            this.logger.LogInformation("Found potential Target: {0}", addressString);
        }
    }
}