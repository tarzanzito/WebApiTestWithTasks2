
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

//"Logging": {
//    "RoundTheCodeFile": {
//        "Options": {
//            "FolderPath": "d:\\Zlogs",
//        "FilePath": "log_{date}.log"
//        }
//    }

namespace WebApplication1
{
    // RoundTheCodeFileLoggerOptions.cs
    public sealed class RoundTheCodeFileLoggerOptions //sealed
    {
        public string FilePath { get; set; } = string.Empty;

        public string FolderPath { get; set; } = string.Empty;
    }

    /////////////////////////////////////////////////////////////////////

    // RoundTheCodeFileLoggerProvider.cs
    [ProviderAlias("RoundTheCodeFile")]
    public sealed class RoundTheCodeFileLoggerProvider : ILoggerProvider
    {
        public readonly RoundTheCodeFileLoggerOptions Options;

        public RoundTheCodeFileLoggerProvider(IOptions<RoundTheCodeFileLoggerOptions> options) //mandatory with "IOptions<>"
        {
            Options = options.Value;

            if (Options.FolderPath == string.Empty)
                return;

            if (Directory.Exists(Options.FolderPath))
                return;
            
            Directory.CreateDirectory(Options.FolderPath);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new RoundTheCodeFileLogger(this);
        }

        public void Dispose()
        {
        }
    }

    /////////////////////////////////////////////////////////////////////

    // RoundTheCodeFileLogger.cs
    public sealed class RoundTheCodeFileLogger : ILogger//sealed
    {
        private readonly RoundTheCodeFileLoggerProvider _roundTheCodeLoggerFileProvider; //protected

        private RoundTheCodeFileLogger() { }

        //public RoundTheCodeFileLogger([NotNull] RoundTheCodeFileLoggerProvider roundTheCodeLoggerFileProvider)
        public RoundTheCodeFileLogger(RoundTheCodeFileLoggerProvider roundTheCodeLoggerFileProvider)
        {
            _roundTheCodeLoggerFileProvider = roundTheCodeLoggerFileProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var fullFilePath = _roundTheCodeLoggerFileProvider.Options.FolderPath + "/" + _roundTheCodeLoggerFileProvider.Options.FilePath.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"));
            var logRecord = string.Format("{0} [{1}] {2} {3}", "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]", logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : "");

            using var streamWriter = new StreamWriter(fullFilePath, true);
            
            streamWriter.WriteLine(logRecord);
            
        }
    }

    /////////////////////////////////////////////////////////////////////

    public static class RoundTheCodeFileLoggerExtensions
    {
        public static ILoggingBuilder AddRoundTheCodeFileLogger(this ILoggingBuilder builder, Action<RoundTheCodeFileLoggerOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, RoundTheCodeFileLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}

//base
//https://www.roundthecode.com/dotnet-tutorials/create-your-own-logging-provider-to-log-to-text-files-in-net-core




