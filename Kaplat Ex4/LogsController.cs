using System.Linq;
using System.Web.Http;
using NLog;
using NLog.Config;

namespace BookStore.Controllers
{
    public class LogsController : BooksController
    {       
        [HttpGet]
        [Route("logs/level")]
        public IHttpActionResult GetLogLevel([FromUri] string loggerName)
        {
            Logger logger = LogManager.GetLogger(loggerName);
            if (logger == null)
            {
                return BadRequest($"Logger {loggerName} not found.");
            }

            var loggingRule = LogManager.Configuration.LoggingRules.FirstOrDefault(r => r.LoggerNamePattern == loggerName);
            if (loggingRule == null)
            {
                return BadRequest($"Logging rule for {loggerName} not found.");
            }

            var level = loggingRule.Levels.FirstOrDefault()?.Name.ToUpper();
            if (level == null)
            {
                return BadRequest($"Log level for {loggerName} could not be determined.");
            }

            return Ok(level);
        }

        [HttpPut]
        [Route("logs/level")]
        public IHttpActionResult SetLogLevel([FromUri(Name = "logger-name")] string loggerName, 
            [FromUri(Name = "logger-level")] string loggerLevel)
        {
            Logger logger = LogManager.GetLogger(loggerName);
            if (logger == null)
            {
                return BadRequest($"Logger {loggerName} not found.");
            }

            LogLevel level;
            switch (loggerLevel.ToUpper())
            {
                case "ERROR":
                    level = LogLevel.Error;
                    break;
                case "INFO":
                    level = LogLevel.Info;
                    break;
                case "DEBUG":
                    level = LogLevel.Debug;
                    break;
                default:
                    return BadRequest($"Invalid log level {loggerLevel}");
            }

            var loggingRule = LogManager.Configuration.LoggingRules.FirstOrDefault(r => r.LoggerNamePattern == loggerName);
            if (loggingRule == null)
            {
                return BadRequest($"Logging rule for {loggerName} not found.");
            }

            loggingRule.SetLoggingLevels(level, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();

            return Ok(level.Name.ToUpper());
        }
    }
}
