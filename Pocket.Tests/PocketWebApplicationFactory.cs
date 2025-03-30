using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace Pocket;

public class PocketWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(b =>
            {
                b.Add(new PostgresConfigurationSource());
            });
    }

    private class PostgresConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new PostgresConfigurationProvider();
        }
    }

    private class PostgresConfigurationProvider : ConfigurationProvider
    {
        private static readonly TaskFactory TaskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public override void Load()
        {
            // Until the asynchronous configuration provider is available,
            // we use the TaskFactory to spin up a new task that handles the work:
            // https://github.com/dotnet/runtime/issues/79193
            // https://github.com/dotnet/runtime/issues/36018
            TaskFactory.StartNew(LoadAsync)
                .Unwrap()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public async Task LoadAsync()
        {
            var postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17")
                .Build();

            await postgreSqlContainer.StartAsync()
                .ConfigureAwait(false);

            Set("ConnectionStrings:DefaultConnection", postgreSqlContainer.GetConnectionString());
        }
    }
}
