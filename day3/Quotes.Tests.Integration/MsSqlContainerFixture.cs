using Testcontainers.MsSql;

namespace Quotes.Tests.Integration;

public class MsSqlContainerFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; } =
        new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}