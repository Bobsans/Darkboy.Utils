using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Darkboy.Extensions.Logging;

public static class EnhancedConsoleExtension {
    public static ILoggingBuilder AddEnhancedConsole(this ILoggingBuilder builder) {
        builder.AddConsoleFormatter<EnhancedConsoleFormatter, ConsoleFormatterOptions>();
        builder.AddConsole(options => { options.FormatterName = EnhancedConsoleFormatter.NAME; });
        return builder;
    }
}
