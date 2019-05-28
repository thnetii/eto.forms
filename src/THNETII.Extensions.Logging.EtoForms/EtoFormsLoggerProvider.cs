using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace THNETII.Extensions.Logging.EtoForms
{
    public class EtoFormsLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, EtoFormsLogger> loggers =
            new ConcurrentDictionary<string, EtoFormsLogger>(StringComparer.OrdinalIgnoreCase);

        private readonly Lazy<LoggerExternalScopeProvider> fallbackScopeProvider =
            new Lazy<LoggerExternalScopeProvider>(() => new LoggerExternalScopeProvider());
        private readonly Func<string, LogLevel, bool> filter;
        private IDisposable optionsReloadToken;
        private IExternalScopeProvider scopeProvider;
        private bool includeScopes;
        private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;

        public IExternalScopeProvider ScopeProvider
        {
            get => includeScopes ? scopeProvider ?? fallbackScopeProvider.Value : null;
            set => SetScopeProvider(value);
        }

        public EtoFormsLoggerProvider(IOptionsMonitor<EtoFormsLoggerOptions> options) : base()
        {
            filter = trueFilter;
            optionsReloadToken = options.OnChange(OnLoggerOptionsChange);
            OnLoggerOptionsChange(options.CurrentValue);
        }

        public ILogger CreateLogger(string categoryName) =>
            loggers.GetOrAdd(categoryName, CreateLoggerImpl);

        private EtoFormsLogger CreateLoggerImpl(string name)
        {
            return new EtoFormsLogger(name);
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
            this.scopeProvider = scopeProvider;

        private void OnLoggerOptionsChange(EtoFormsLoggerOptions options)
        {
            includeScopes = options.IncludeScopes;
            var scopeProvider = ScopeProvider;
            foreach (var logger in loggers.Values)
            {
                logger.ScopeProvider = scopeProvider;
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EtoFormsLoggerProvider()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            System.Threading.Interlocked.Exchange(ref optionsReloadToken, null)?.Dispose();
        }
        #endregion
    }

    public class EtoFormsLoggerOptions
    {
        public bool IncludeScopes { get; set; }
    }

    public class EtoFormsLogger : ILogger
    {
        private readonly string name;

        public EtoFormsLogger(string name)
        {
            this.name = string.IsNullOrWhiteSpace(name) ? nameof(EtoFormsLogger) : name;
        }

        public IExternalScopeProvider ScopeProvider { get; internal set; }

        public IDisposable BeginScope<TState>(TState state) =>
            ScopeProvider?.Push(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter?.Invoke(state, exception);
            var args = state as IEnumerable<KeyValuePair<string, object>> ?? new[] { new KeyValuePair<string, object>(nameof(state), state) };
            if (exception is Exception)
                args = args.Concat(new[] { new KeyValuePair<string, object>('$' + nameof(exception), exception) });
            Log(logLevel, eventId, message, args);
        }

        protected void Log(LogLevel logLevel, EventId eventId, string message, IEnumerable<KeyValuePair<string, object>> args)
        {
            throw new NotImplementedException();
        }
    }

    public class EtoFormsLoggerStackLayoutItem : Eto.Forms.StackLayoutItem
    {
        public EtoFormsLoggerStackLayoutItem() : base()
        {
            
        }
    }
}
