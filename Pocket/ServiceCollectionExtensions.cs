using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Pocket.Application;
using Pocket.Infrastructure.CookiePasswordHistory;
using Pocket.Infrastructure.Format;

namespace Pocket;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPocket(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<PocketAuthenticationOptions>()
            .BindConfiguration("Authentication")
            .ValidateDataAnnotations()
            .Configure(o => { o.AllowedTokenHashSet = o.AllowedTokenHashes.ToFrozenSet(StringComparer.Ordinal); });

        services.AddOptions<AccessTokenOptions>()
            .PostConfigure<IDataProtectionProvider>(
                [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
                (options, provider) =>
                {
                    if (options.TicketDataFormat is null)
                    {
                        var dataProtector = provider.CreateProtector("Pocket.Authentication.AccessToken");
                        options.TicketDataFormat = new TicketDataFormat(dataProtector);
                    }
                });

        services.AddOptions<PasswordHistoryOptions>()
            .PostConfigure<IDataProtectionProvider>(
                [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
                (options, provider) =>
                {
                    if (options.PasswordHistoryFormat is null)
                    {
                        options.PasswordHistoryFormat = new SecureDataFormat<PasswordHistory>(
                            new MemoryPackDataSerializer<PasswordHistory>(),
                            provider.CreateProtector("Pocket.PasswordHistory")
                        );
                    }
                });

        services.AddScoped<AccessTokenHandler>();
        services.AddScoped<SecureNoteService>();
        services.AddScoped<PasswordHistoryHandler>();

        return services;
    }
}