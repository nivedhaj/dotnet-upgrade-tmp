using ct._Core.Repo.Dto;
using ct._Core.Util;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

namespace ct._Core.Service
{
    /// <summary>
    /// RabbitMQService Interface
    /// </summary>
    public interface IRabbitMQService
    {
        /// <summary>
        /// Push a message on to the q
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="routingKey"></param>
        /// <param name="xchg"></param>
        /// <param name="topic"></param>
        void Push(object msg, string routingKey, string xchg, string topic = "");
    }
    /// <summary>
    /// RabbitMQ Service
    /// </summary>
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly string host_name;
        private readonly string virtual_host;
        private readonly string user_name;
        private readonly string password;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        public RabbitMQService(IConfiguration configuration)
        {
            _config = configuration;
            host_name = _config["RabbitMQ:Host"];
            virtual_host = _config["RabbitMQ:Virtual.Host"];
            user_name = _config["RabbitMQ:UserName"];
            password = _config["RabbitMQ:Password"];
        }
        /// <summary>
        /// Push generic
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="routingKey"></param>
        /// <param name="xchg"></param>
        /// <param name="topic"></param>
        public void Push(object obj, string routingKey, string xchg, string topic = "")
        {
            var _topic = string.IsNullOrEmpty(topic) ? routingKey : topic.ToLower();
            var connectionFactory = new ConnectionFactory()
            {
                VirtualHost = virtual_host,
                HostName = host_name,
                UserName = user_name,
                Password = password
            };
            using (var connection = connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var props = channel.CreateBasicProperties();
                    props.DeliveryMode = 2; //persistent
                    props.CorrelationId = _topic;
                    var exchange_type = routingKey.Contains(".") ? ExchangeType.Topic : ExchangeType.Direct;
                    var address = new PublicationAddress(exchange_type, xchg, routingKey);
                    var payload = Helper.GetBytesFromObject(obj);
                    channel.BasicPublish(address, props, payload);
                    // channel.Close();
                }
            }
        }
        /// <summary>
        /// Push Email
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueEmail(EmailJobDto dto)
        {
            var routingKey = "ct_r_email_generic";
            var exchange = "ct_x";
            Push(dto, routingKey, exchange, dto.topic);
        }
        /// <summary>
        /// Push Notification
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueNotification(NotificationJobDto dto)
        {
            var routingKey = "ct_r_push_notification";
            var exchange = "ct_x";
            Push(dto, routingKey, exchange);
        }
        /// <summary>
        /// Push SMS
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueSMS(Job dto)
        {
            var routingKey = "ct_r_sms";
            var exchange = "ct_x";
            Push(dto, routingKey, exchange, dto.topic);
        }
        /// <summary>
        /// enque student sync job
        /// </summary>
        /// <param name="job"></param>
        public void EnqueStudentSyncJob(Job job)
        {
            var routingKey = "student.sync";
            var exchange = "ct_xt";
            Push(job, routingKey, exchange, job.topic);
        }
        /// <summary>
        /// Student created job
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentCreatedJob(Job dto)
        {
            var routingKey = "student.created";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }

        /// <summary>
        /// Student created job handled by fee worker
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentCreatedFeeWorkerJob(Job dto)
        {
            var routingKey = "fee.student.created";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }
        /// <summary>
        /// Student status updated job
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentStatusUpdatedJob(Job dto)
        {
            var routingKey = "student.status.updated";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }

         public void EnqueFWStudentStatusUpdatedJob(Job dto)
        {
            var routingKey = "fee.student.status.updated";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }

        /// <summary>
        /// Student disabled job
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentDisabledJob(Job dto)
        {
            var routingKey = "student.disabled";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }
        /// <summary>
        /// Student enabled job
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentEnabledJob(Job dto)
        {
            var routingKey = "student.enabled";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }
        /// <summary>
        /// Student updated job
        /// </summary>
        /// <param name="dto"></param>
        public void EnqueStudentUpdatedJob(Job dto)
        {
            var routingKey = "student.updated";
            var exchange = "ct_xt";
            Push(dto, routingKey, exchange);
        }
        /// <summary>
        /// Dispose the connection
        /// </summary>
        public void Dispose()
        {
            //ToDo: What to do here?
        }
        /// <summary>
        /// enque a job
        /// </summary>
        /// <param name="job"></param>
        public void EnqueJob(Job job)
        {
            var routingKey = "ct_r_jobs";
            var exchange = "ct_x";
            Push(job, routingKey, exchange);
        }

        /// <summary>
        /// enque a log
        /// </summary>
        /// <param name="message"></param>
        public void EnqueLog(string message)
        {
            var routingKey = "ct_r_logs";
            var exchange = "ct_x";
            Push(message, routingKey, exchange);
        }
    }

    /// <summary>
    /// The naming conventions use snake_casing (keeping in view the background processes that handle the job
    /// </summary>
    [Serializable]
    public class Job
    {
        /// <summary>
        /// id
        /// </summary>
        /// <value></value>
        public string id { get; set; }
        /// <summary>
        /// job title
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Payload
        /// </summary>
        public dynamic payload { get; set; }
        /// <summary>
        /// Meta data
        /// </summary>
        public dynamic meta { get; set; }
        /// <summary>
        /// user id
        /// </summary>
        /// <value></value>
        public string user_id { get; set; }
        /// <summary>
        /// client id
        /// </summary>
        /// <value></value>
        public string client_id { get; set; }
        /// <summary>
        /// Unix Timestamp
        /// </summary>
        public long ts { get; set; }
        /// <summary>
        /// Topic
        /// </summary>
        /// <value></value>
        public string topic { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        /// <value></value>
        public dynamic token { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public Job()
        {
            ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
