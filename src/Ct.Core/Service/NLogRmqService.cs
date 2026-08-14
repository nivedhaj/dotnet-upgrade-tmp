using NLog;
using NLog.Targets;
using System;
using System.Diagnostics;

namespace ct._Core.Service
{
    /// <summary>
    /// NLog RabbitMQ Target
    /// </summary>
    [Target("RMQ")]
    public class NLogRmqService : TargetWithLayout
    {
        private readonly RabbitMQService _rmqService;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rmqService"></param>
        public NLogRmqService(RabbitMQService rmqService)
        {
            _rmqService = rmqService;
        }
        /// <summary>
        /// Write to log
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                string logMessage = Layout.Render(logEvent);
                _rmqService.EnqueLog(logMessage);
                Debug.WriteLine("RMQTarget/Write using IRabbitMQService ...");
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// Close target
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();
        }
    }
}