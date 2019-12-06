using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            const string exceptionName = "$" + nameof(exception);
            const string stateName = nameof(state);

            var message = formatter?.Invoke(state, exception);

            IEnumerable<KeyValuePair<string, object>> args = null;
            switch (state)
            {
                case IDictionary<string, object> dict:
                    if (exception is Exception)
                        dict[exceptionName] = exception;
                    args = dict;
                    break;
                case ICollection<KeyValuePair<string, object>> collection:
                    if (exception is Exception)
                        collection.Add(WrapExceptionInKvp(exception));
                    args = collection;
                    break;
                case IEnumerable<KeyValuePair<string, object>> enumerable:
                    args = enumerable;
                    if (exception is Exception)
                        args = args.Concat(WrapExceptionInKvpArray(exception));
                    break;
                default:
                    var stateObj = (object)state;
                    if (stateObj is null)
                    {
                        if (exception is Exception)
                            args = WrapExceptionInKvpArray(exception);
                    }
                    else if (exception is null)
                        args = new[] { WrapStateInKvp(stateObj) };
                    else
                        args = new[]
                        {
                            WrapStateInKvp(stateObj),
                            WrapExceptionInKvp(exception)
                        };
                    break;
            }
            Log(logLevel, eventId, message, args);

            KeyValuePair<string, object>[] WrapExceptionInKvpArray(Exception e) =>
                new[] { WrapExceptionInKvp(e) };
            KeyValuePair<string, object> WrapExceptionInKvp(Exception e) =>
                new KeyValuePair<string, object>(exceptionName, e);
            KeyValuePair<string, object> WrapStateInKvp(object value) =>
                new KeyValuePair<string, object>(stateName, value);
        }

        protected void Log(LogLevel logLevel, EventId eventId, string message, IEnumerable<KeyValuePair<string, object>> args)
        {
            throw new NotImplementedException();
        }
    }

    public class EtoFormsLogItemControl : Eto.Forms.GroupBox
    {
        public EtoFormsLogItemControl(DateTime timestamp, LogLevel logLevel,
            string category, EventId eventId, string message)
            : base()
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            var timestampText = timestamp.ToString(invariant);
            var logLevelText = logLevel.ToString();
            var eventIdValueText = eventId.Id.ToString(invariant);
            var eventIdText = string.IsNullOrWhiteSpace(eventId.Name)
                ? eventIdValueText
                : $"{eventIdValueText}: {eventId.Name}";
            var headingText = $"{category}[{eventIdText}]";

            var headingTableLayout = new Eto.Forms.TableLayout();
            

            var logLevelLabel = new Eto.Forms.Label { Text = logLevelText };
            var messageLabel = new Eto.Forms.Label
            {
                Text = message,
                Wrap = Eto.Forms.WrapMode.Word
            };

            var contentStackLayout = new Eto.Forms.StackLayout(
                new Eto.Forms.StackLayoutItem(logLevelLabel),
                new Eto.Forms.StackLayoutItem(messageLabel)
                )
            {
                HorizontalContentAlignment = Eto.Forms.HorizontalAlignment.Left,
                Orientation = Eto.Forms.Orientation.Vertical
            };

            Text = timestampText;
            Content = contentStackLayout;
        }
    }
}
