using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public enum ApplicationMode
{
    Listen,
    Discovery
}

public class Settings
{
    public string Interface { get; set; }
    public ApplicationMode Mode { get; set; }
    public string MqttHost { get; set; }
    public int MqttPort { get; set; }
    public string MqttClientId { get; set; }
    public List<DeviceMap> DeviceMap { get; set; }
    public byte[] TogglePacket { get; set; }
}
