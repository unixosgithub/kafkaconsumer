using Confluent.Kafka;
using Google.Api;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using System.Diagnostics.Eventing.Reader;

namespace kafkaconsumer.Logging
{
    public class Logger : ILogger
    {
        private LoggingServiceV2Client _logClient;
        private MonitoredResource _resource;
        private IDictionary<string, string> _entryLabels;

        public Logger()
        {
            _logClient = LoggingServiceV2Client.Create();
            _resource = new MonitoredResource
            {
                Type = "global"
            };

            // Create dictionary object to add custom labels to the log entry.
            _entryLabels = new Dictionary<string, string>();
            _entryLabels.Add("size", "large");
            _entryLabels.Add("color", "blue");
        }

        public async Task LogInfo(string projectId, string message)
        {
            // Prepare new log entry.
            LogEntry logEntry = new LogEntry();
            string logId = "kafkaconsumer-infolog";
            LogName logName = new LogName(projectId, logId);
            logEntry.LogNameAsLogName = logName;
            logEntry.Severity = LogSeverity.Info;

            // Create log entry message.            
            string messageId = DateTime.Now.Millisecond.ToString();
            Type myType = typeof(Logger);
            string entrySeverity = logEntry.Severity.ToString().ToUpper();
            logEntry.TextPayload =
                $"{messageId} {entrySeverity} {myType.Namespace}.LogInfo - {message}";                       

            // Add log entry to collection for writing. Multiple log entries can be added.
            IEnumerable<LogEntry> logEntries = new LogEntry[] { logEntry };

            // Write new log entry.
            _logClient.WriteLogEntries(logName, _resource, _entryLabels, logEntries);
        }

        public async Task LogError(string projectId, string message)
        {
            // Prepare new log entry.
            LogEntry logEntry = new LogEntry();
            string logId = "kafkaconsumer-errlog";
            LogName logName = new LogName(projectId, logId);
            logEntry.LogNameAsLogName = logName;
            logEntry.Severity = LogSeverity.Error;

            // Create log entry message.            
            string messageId = DateTime.Now.Millisecond.ToString();
            Type myType = typeof(Logger);
            string entrySeverity = logEntry.Severity.ToString().ToUpper();
            logEntry.TextPayload =
                $"{messageId} {entrySeverity} {myType.Namespace}.LogError - {message}";

            // Add log entry to collection for writing. Multiple log entries can be added.
            IEnumerable<LogEntry> logEntries = new LogEntry[] { logEntry };

            // Set the color to Red for errors
            _entryLabels["color"] = "red";
            // Write new log entry.
            _logClient.WriteLogEntries(logName, _resource, _entryLabels, logEntries);
        }
    }
}
