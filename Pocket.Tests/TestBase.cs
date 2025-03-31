using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pocket.Data;

namespace Pocket;

public abstract class TestBase
{
    [SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method")]
    protected PocketWebApplicationFactory WebApplicationFactory { get; private set; }

    [SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method")]
    private AsyncServiceScope? scope;

    protected IServiceProvider Services => scope!.Value.ServiceProvider;

    [OneTimeSetUp]
    public virtual async Task OneTimeSetUpAsync()
    {
        OneTimeSetUp();
        await using var setupScope = WebApplicationFactory.Services.CreateAsyncScope();
        var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public virtual void OneTimeSetUp()
    {
        WebApplicationFactory = new PocketWebApplicationFactory();
    }

    [SetUp]
    public virtual void SetUpAsync()
    {
        SetUp();
    }

    public virtual void SetUp()
    {
        scope = WebApplicationFactory.Services.CreateAsyncScope();
    }

    [TearDown]
    public virtual async Task TearDownAsync()
    {
        TearDown();
        if (scope is not null)
        {
            await scope.Value.DisposeAsync();
        }
    }

    protected virtual void TearDown()
    {
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDownAsync()
    {
        OneTimeTearDown();
        await WebApplicationFactory.DisposeAsync();
    }

    protected virtual void OneTimeTearDown()
    {
    }
}
