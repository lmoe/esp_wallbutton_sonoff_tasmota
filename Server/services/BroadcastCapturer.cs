using System.Linq;
using SharpPcap;
using SharpPcap.LibPcap;

public class BroadcastCapturer : IBroadcastCapturer
{
    public string Filter { get; set; }
    public event ActionFrameArrived OnActionFrameArrived;

    private ICaptureDevice CaptureDevice { get; set; }
    private Settings Settings { get; set; }

    public BroadcastCapturer(Settings settings)
    {
        this.Settings = settings;
    }

    public void StartCapture()
    {
        this.CaptureDevice = CaptureDeviceList.Instance
            .Single(x => x.Name == this.Settings.Interface);

        this.CaptureDevice.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);

        var livePcapDevice = this.CaptureDevice as LibPcapLiveDevice;

        livePcapDevice.Open(DeviceMode.Promiscuous, 50, MonitorMode.Active);

        this.CaptureDevice.Filter = this.Filter;

        this.CaptureDevice.StartCapture();
    }

    private void OnPacketArrival(object sender, CaptureEventArgs e)
    {
        var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

        if (packet.PayloadPacket is PacketDotNet.Ieee80211.ActionFrame == false)
        {
            return;
        }

        var actionFrame = (PacketDotNet.Ieee80211.ActionFrame)packet.PayloadPacket;

        if (this.OnActionFrameArrived != null)
        {
            this.OnActionFrameArrived(actionFrame);
        }
    }
}
