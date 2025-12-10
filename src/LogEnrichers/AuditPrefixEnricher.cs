namespace LAPS_WebUI.LogEnrichers;

using Serilog.Core;
using Serilog.Events;

public class AuditPrefixEnricher : ILogEventEnricher
{
    private const string PropertyName = "AuditPrefix";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("Audit", out LogEventPropertyValue? auditProp) &&
            auditProp is ScalarValue { Value: bool and true })
        {
            LogEventProperty auditPrefix = propertyFactory.CreateProperty(PropertyName, "[AUDIT] ");
            logEvent.AddPropertyIfAbsent(auditPrefix);
        }
        else
        {
            LogEventProperty empty = propertyFactory.CreateProperty(PropertyName, "");
            logEvent.AddPropertyIfAbsent(empty);
        }
    }
}