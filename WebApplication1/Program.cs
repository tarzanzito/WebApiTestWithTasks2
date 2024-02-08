
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace WebApplication1
{
    public sealed class Program
    {
        private static ILogger<Program>? _logger = null;

        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                //

                builder.Services.AddSingleton<IStatisticsInfo, StatisticsInfo>();

                builder.Services.AddLogging(loggingBuilder =>
                {
                    //To File
                    loggingBuilder.AddRoundTheCodeFileLogger(options =>
                    {
                        builder.Configuration.GetSection("Logging").GetSection("RoundTheCodeFile").GetSection("Options").Bind(options);
                    });

                    // Provided by Microsoft.Extensions.Logging.Console : differences?
                    //loggingBuilder.AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss] ");
                    //loggingBuilder.AddJsonConsole(options => { });
                    //loggingBuilder.AddSystemdConsole(options => { });
                    loggingBuilder.AddSystemdConsole();
                });

                var app = builder.Build();

                //inject
                _logger = app.Services.GetService<ILogger<Program>>();
                if (_logger == null)
                    throw new Exception("Logger is null");

                LogWrite("Program Started...");

                //

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();

                LogWrite("Program Finished...");
            }
            catch (Exception ex)
            {
                if (_logger == null)
                    Console.WriteLine($"Program Main Error: [{ex.Message}]");
                else
                    LogWrite(LogLevel.Error, "Program Main Error: {ex.Message}", ex.Message);
            }
        }

        private static void LogWrite(LogLevel logLevel, string? msg, params object?[] args)
        {
            _logger?.Log(logLevel, msg, args);
        }

        private static void LogWrite(string? msg, params object?[] args)
        {
            LogWrite(LogLevel.Information, msg, args);
        }
    }
}


//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.AddSeq();
//    //Configuration.GetSection("Seq")
//});
/////////////////////////////////////////////////////

//public class Xpto
//{
//    public static void Main(string[] args)
//    {
//        Log.Logger = new LoggerConfiguration()
//            .MinimumLevel.Debug()
//            .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "C:\\logs\\log-{Date}.txt"),
//                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] [{EventId}] {Message}{NewLine}{Exception}")
//            //.ReadFrom.Configuration(Configuration)
//            .CreateLogger();
//        Log.Logger.Information("test");



//        Log.Logger = new LoggerConfiguration()
//        //.MinimumLevel.Debug()
//        //.WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "C:\\logs\\log-{Date}.txt"),
//        //                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] [{EventId}] {Message}{NewLine}{Exception}")
//        .ReadFrom.Configuration(Configuration)
//        .CreateLogger();
//    }

//}
