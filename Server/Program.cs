using System;
using System.Linq;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using SharpPcap.LibPcap;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client;
using MQTTnet;
using System.Threading.Tasks;

namespace wallbutton_listener
{

    class Program
    {
        public static async Task Main(string[] args)
        {
            var settings = InitializeSettings();
            var services = await InitializeServices(settings);

            var executor = services.GetRequiredService<IExecutor>();

            executor.Work()
                .Wait();
        }

        static Settings InitializeSettings()
        {
            var configurationBuilder = new ConfigurationBuilder();
            var builder = configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var settings = new Settings();

            configuration.GetSection("Main").Bind(settings);

            return settings;
        }

        async static Task<IServiceProvider> InitializeServices(Settings settings)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(settings);
            serviceCollection.AddSingleton<IExecutor, Executor>();
            serviceCollection.AddLogging(x => x.AddConsole());
            serviceCollection.AddSingleton<IBroadcastCapturer, BroadcastCapturer>();

            if (settings.Mode == ApplicationMode.Discovery)
            {
                serviceCollection.AddSingleton<IMonitor, DiscoveryMonitor>();
            }
            else
            {
                serviceCollection.AddSingleton<IMonitor, ListenerMonitor>();
            }

            var mqttClient = await InitializeMqtt(settings);

            serviceCollection.AddSingleton<IManagedMqttClient>(mqttClient);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }

        async static Task<IManagedMqttClient> InitializeMqtt(Settings settings)
        {
            var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId(settings.MqttClientId)
                .WithTcpServer(settings.MqttHost, settings.MqttPort)
                .Build())
            .Build();

            var mqttClient = new MqttFactory()
                .CreateManagedMqttClient();

            await mqttClient.StartAsync(options);

            return mqttClient;
        }
    }
}
