using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Darkboy.Extensions.Logging;

public class EnhancedConsoleFormatter() : Microsoft.Extensions.Logging.Console.ConsoleFormatter(NAME) {
    public const string NAME = nameof(EnhancedConsoleFormatter);

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter) {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        if (logEntry.Exception == null && string.IsNullOrEmpty(message)) {
            return;
        }

        var logLevel = logEntry.LogLevel;
        var logLevelColors = GetLogLevelConsoleColors(logLevel);

        textWriter.Write($"{ColorMessage($"[{DateTimeOffset.UtcNow:dd-MM-yyyy HH:mm:ss.fffzzz}]", null, ConsoleColor.DarkBlue)} ");
        textWriter.Write($"{ColorMessage(GetLogLevelString(logEntry.LogLevel), logLevelColors.Background, logLevelColors.Foreground)} ");

        CreateDefaultLogMessage(textWriter, logEntry, message);
    }

    private static void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message) {
        var exception = logEntry.Exception;

        textWriter.Write($"{ColorMessage($"[{logEntry.Category}]", null, ConsoleColor.Gray)} ");

        WriteMessage(textWriter, message);

        if (exception != null) {
            textWriter.Write(" ");
            textWriter.Write(exception.ToString());
        }

        textWriter.Write(Environment.NewLine);
    }

    private static void WriteMessage(TextWriter textWriter, string message) {
        if (!string.IsNullOrEmpty(message)) {
            textWriter.Write(message.Replace(Environment.NewLine, " "));
        }
    }

    private static string GetLogLevelString(LogLevel logLevel) {
        return logLevel switch {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            LogLevel.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private static ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel) {
        return logLevel switch {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, null),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, null),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, null),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, null),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    private static string ColorMessage(string message, ConsoleColor? background, ConsoleColor? foreground) {
        if (Console.IsOutputRedirected) {
            return message;
        }

        var builder = new StringBuilder();
        if (background.HasValue) {
            builder.Append(GetBackgroundColorEscapeCode(background.Value));
        }

        if (foreground.HasValue) {
            builder.Append(GetForegroundColorEscapeCode(foreground.Value));
        }

        builder.Append(message);
        if (foreground.HasValue) {
            builder.Append(DEFAULT_FOREGROUND_COLOR);
        }

        if (background.HasValue) {
            builder.Append(DEFAULT_BACKGROUND_COLOR);
        }

        return builder.ToString();
    }

    private const string DEFAULT_FOREGROUND_COLOR = "\e[39m\e[22m";
    private const string DEFAULT_BACKGROUND_COLOR = "\e[49m";

    private static string GetForegroundColorEscapeCode(ConsoleColor color) {
        return color switch {
            ConsoleColor.Black => "\e[30m",
            ConsoleColor.DarkRed => "\e[31m",
            ConsoleColor.DarkGreen => "\e[32m",
            ConsoleColor.DarkYellow => "\e[33m",
            ConsoleColor.DarkBlue => "\e[34m",
            ConsoleColor.DarkMagenta => "\e[35m",
            ConsoleColor.DarkCyan => "\e[36m",
            ConsoleColor.Gray => "\e[37m",
            ConsoleColor.Red => "\e[1m\e[31m",
            ConsoleColor.Green => "\e[1m\e[32m",
            ConsoleColor.Yellow => "\e[1m\e[33m",
            ConsoleColor.Blue => "\e[1m\e[34m",
            ConsoleColor.Magenta => "\e[1m\e[35m",
            ConsoleColor.Cyan => "\e[1m\e[36m",
            ConsoleColor.White => "\e[1m\e[37m",
            _ => DEFAULT_FOREGROUND_COLOR
        };
    }

    private static string GetBackgroundColorEscapeCode(ConsoleColor color) {
        return color switch {
            ConsoleColor.Black => "\e[40m",
            ConsoleColor.DarkRed => "\e[41m",
            ConsoleColor.DarkGreen => "\e[42m",
            ConsoleColor.DarkYellow => "\e[43m",
            ConsoleColor.DarkBlue => "\e[44m",
            ConsoleColor.DarkMagenta => "\e[45m",
            ConsoleColor.DarkCyan => "\e[46m",
            ConsoleColor.Gray => "\e[47m",
            _ => DEFAULT_BACKGROUND_COLOR
        };
    }

    private readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background) {
        public ConsoleColor? Foreground { get; } = foreground;
        public ConsoleColor? Background { get; } = background;
    }
}
