using System.Threading.Tasks;

public class Executor : IExecutor
{
    private readonly IMonitor monitorService;
    public Executor(IMonitor monitorService)
    {
        this.monitorService = monitorService;
    }

    public async Task Work()
    {
        await this.monitorService.Initialize();
        await this.monitorService.Listen();
    }
}