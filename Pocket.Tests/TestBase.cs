using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Pocket;

public abstract class TestBase
{
    protected PocketWebApplicationFactory WebApplicationFactory { get; private set; }

    [SuppressMessage("Structure", "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method")]
    private AsyncServiceScope? scope;

    protected IServiceProvider Services => scope!.Value.ServiceProvider;

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        WebApplicationFactory = new PocketWebApplicationFactory();
    }

    [SetUp]
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
