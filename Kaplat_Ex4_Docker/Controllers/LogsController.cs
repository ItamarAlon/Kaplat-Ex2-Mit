using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Linq;

namespace BookStore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;

        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("level")]
        public IActionResult GetLogLevel([FromQuery] string loggerName)
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

        [HttpPut("level")]
        public IActionResult SetLogLevel([FromQuery(Name = "logger-name")] string loggerName,
            [FromQuery(Name = "logger-level")] string loggerLevel)
        {
            Logger logger = LogManager.GetLogger(loggerName);
            if (logger == null)
            {
                return BadRequest($"Logger {loggerName} not found.");
            }

            NLog.LogLevel level;
            switch (loggerLevel.ToUpper())
            {
                case "ERROR":
                    level = NLog.LogLevel.Error;
                    break;
                case "INFO":
                    level = NLog.LogLevel.Info;
                    break;
                case "DEBUG":
                    level = NLog.LogLevel.Debug;
                    break;
                default:
                    return BadRequest($"Invalid log level {loggerLevel}");
            }

            var loggingRule = LogManager.Configuration.LoggingRules.FirstOrDefault(r => r.LoggerNamePattern == loggerName);
            if (loggingRule == null)
            {
                return BadRequest($"Logging rule for {loggerName} not found.");
            }

            loggingRule.SetLoggingLevels(level, NLog.LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();

            return Ok(level.Name.ToUpper());
        }
    }
}
