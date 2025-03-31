namespace Pocket;

public static class HostEnvironmentEnvExtensions
{
    public static bool IsDemo(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment("Demo");
    }
}