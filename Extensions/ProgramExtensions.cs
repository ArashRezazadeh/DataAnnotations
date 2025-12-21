using Serilog;
using Serilog.Events;

namespace DataAnnotations.Extensions; 


public static class ProgramExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        builder.Host.UseSerilog();
        
        return builder;

        // Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
        // builder.Host.UseSerilog((context, services, configuration) => configuration
        //     .ReadFrom.Configuration(context.Configuration)
        //     .ReadFrom.Services(services)
        //     .Enrich.FromLogContext()
        //     .WriteTo.Seq(
        //         serverUrl: GetSeqUrl(context.Configuration),
        //         apiKey: GetSeqApiKey(context.Configuration)));
                
        // return builder;
    }

    // private static string GetSeqUrl(IConfiguration configuration)
    // {
    //     var seqUrl = configuration["Seq:Url"];
    //     if (string.IsNullOrEmpty(seqUrl))
    //     {
    //         seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
    //     }
    //     return string.IsNullOrEmpty(seqUrl) ? "http://localhost:5341" : seqUrl;
    // }

    // private static string GetSeqApiKey(IConfiguration configuration)
    // {
    //     return configuration["Seq:ApiKey"] ?? 
    //            Environment.GetEnvironmentVariable("SEQ_API_KEY") ?? 
    //            throw new InvalidOperationException("Seq API key not found.");
    // }

}