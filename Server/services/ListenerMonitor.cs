using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.ManagedClient;
using SharpPcap;
using SharpPcap.LibPcap;

public class ListenerMonitor : IMonitor
{
    private readonly Settings settings;
    private readonly ILogger<ListenerMonitor> logger;
    private readonly IBroadcastCapturer broadcastCapturer;
    private readonly IManagedMqttClient managedMqttClient;

    public ListenerMonitor(Settings settings, ILogger<ListenerMonitor> logger, IBroadcastCapturer broadcastCapturer, IManagedMqttClient managedMqttClient)
    {
        this.settings = settings;
        this.logger = logger;
        this.broadcastCapturer = broadcastCapturer;
        this.managedMqttClient = managedMqttClient;
    }

    public Task Initialize()
    {
        this.logger.LogInformation("Initializing Listener Monitor");

        this.broadcastCapturer.Filter = this.CreatePacketFilter();
        this.broadcastCapturer.OnActionFrameArrived += this.OnPacketArrival;

        return Task.CompletedTask;
    }

    public Task Listen()
    {
        this.broadcastCapturer.StartCapture();

        return Task.CompletedTask;
    }

    private string CreatePacketFilter()
    {
        var filter = "wlan dst FF:FF:FF:FF:FF:FF and (";

        for (var i = 0; i < this.settings.DeviceMap.Count; i++)
        {
            var deviceMap = this.settings.DeviceMap[i];

            filter += string.Format(" wlan src {0} ", deviceMap.Mac.ToLower());

            if (i != this.settings.DeviceMap.Count - 1)
            {
                filter += "or";
            }
        }

        filter += ")";

        return filter;
    }

    private async void OnPacketArrival(PacketDotNet.Ieee80211.ActionFrame actionFrame)
    {
        if (BufferMatchers.FindPatternEndsWith(actionFrame.PayloadData, this.settings.TogglePacket))
        {
            var addressBytes = actionFrame.SourceAddress.GetAddressBytes();
            var addressString = BitConverter
                                    .ToString(addressBytes)
                                    .Replace("-", ":")
                                    .ToLower();

            var mappedTarget = this.settings.DeviceMap
                .SingleOrDefault(x => x.Mac == addressString);

            if (mappedTarget != default(DeviceMap))
            {
                var message = new ManagedMqttApplicationMessage()
                {
                    ApplicationMessage = new MQTTnet.MqttApplicationMessage
                    {
                        Topic = mappedTarget.Topic,
                        Payload = Encoding.UTF8.GetBytes(mappedTarget.Message)
                    }
                };

                await this.managedMqttClient.PublishAsync(message);
            }
        }
    }
}