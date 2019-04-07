using System.Threading.Tasks;

public interface IMonitor 
{
    Task Initialize();
    Task Listen();
}