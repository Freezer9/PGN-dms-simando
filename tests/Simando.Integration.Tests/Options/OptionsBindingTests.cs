using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Simando.Infrastructure.Options;

namespace Simando.Integration.Tests.ConfigurationOptions;

public class OptionsBindingTests
{
    [Fact(DisplayName = "UploadOptions binds MaxSizeMb and computes MaxSizeBytes correctly")]
    public void UploadOptions_BindsAndComputesMaxSizeBytes()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Upload:MaxSizeMb", "50" },
            { "Upload:AllowedTypes:0", ".pdf" },
            { "Upload:AllowedTypes:1", ".png" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<UploadOptions>().Bind(configuration.GetSection("Upload"));
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<UploadOptions>>().Value;

        options.MaxSizeMb.ShouldBe(50);
        options.MaxSizeBytes.ShouldBe(50 * 1024 * 1024);
        options.AllowedTypes.ShouldBe([".pdf", ".png"]);
    }

    [Fact(DisplayName = "AuthOptions binds Password and Lockout policy options correctly")]
    public void AuthOptions_BindsPolicyOptions()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Auth:Password:MinLength", "16" },
            { "Auth:Password:RequireMixed", "true" },
            { "Auth:Lockout:MaxAttempts", "5" },
            { "Auth:Lockout:Minutes", "30" },
            { "Auth:SessionTimeoutMinutes", "120" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<AuthOptions>().Bind(configuration.GetSection("Auth"));
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuthOptions>>().Value;

        options.Password.MinLength.ShouldBe(16);
        options.Password.RequireMixed.ShouldBeTrue();
        options.Lockout.MaxAttempts.ShouldBe(5);
        options.Lockout.Minutes.ShouldBe(30);
        options.SessionTimeoutMinutes.ShouldBe(120);
    }
}
