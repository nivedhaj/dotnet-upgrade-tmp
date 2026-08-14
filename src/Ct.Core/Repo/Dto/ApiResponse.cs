using Newtonsoft.Json;
using System;
using System.Net;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// Api Response. All responses are wrapped in this class
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Http Status
        /// </summary>
        public HttpStatusCode HttpStatus { get; set; }
        /// <summary>
        /// Message Code
        /// </summary>
        public MessageCodeEnum MessageCode { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Payload in json format
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Meta Data
        /// </summary>
        public object Meta { get; set; }
        /// <summary>
        /// Time of response creation
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Response to which reuest Url?
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// Get / Post
        /// </summary>
        public string RequestVerb { get; set; }
        /// <summary>
        /// Paged Results?
        /// </summary>
        public bool PagedResults { get; set; }
        /// <summary>
        /// Current Page
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// Records Per Page
        /// </summary>
        public int RecordsPerPage { get; set; }
        /// <summary>
        /// Total Records
        /// </summary>
        public int TotalRecords { get; set; }
        /// <summary>
        /// Total Pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public ApiResponse()
        {
            this.CreatedOn = DateTime.UtcNow;
        }
        /// <summary>
        /// As Json
        /// </summary>
        /// <returns></returns>
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        /// <summary>
        /// OK
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create200()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.OK;
            return this;
        }
        /// <summary>
        /// Created
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create201()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.Created;
            return this;
        }
        /// <summary>
        /// Accepted
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create202()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.Accepted;
            return this;
        }
        /// <summary>
        /// Redirect
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create302()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.Redirect;
            return this;
        }
        /// <summary>
        /// Bad request
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create400()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.BadRequest;
            return this;
        }
        /// <summary>
        /// Forbidden
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create403()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.Forbidden;
            return this;
        }
        /// <summary>
        /// Not found
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create404()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.NotFound;
            return this;
        }
        /// <summary>
        /// Internal server error
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create500()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.InternalServerError;
            return this;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns></returns>
        public ApiResponse Create501()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatus = HttpStatusCode.NotImplemented;
            return this;
        }
        /// <summary>
        /// With Message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="msgCode"></param>
        /// <returns></returns>
        public ApiResponse WithMessage(string msg, MessageCodeEnum msgCode = MessageCodeEnum.UnDefined)
        {
            this.Message = msg;
            this.MessageCode = msgCode;
            return this;
        }
        /// <summary>
        /// With Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResponse WithData(object data)
        {
            this.Data = data;
            return this;
        }
        /// <summary>
        /// With Total Pages
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public ApiResponse WithTotalPages(int tp)
        {
            this.TotalPages = tp;
            return this;
        }
        /// <summary>
        /// With Records Per PAge
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public ApiResponse WithRecordsPerPage(int rec)
        {
            this.RecordsPerPage = rec;
            return this;
        }
        /// <summary>
        /// With Total Records
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public ApiResponse WithTotalRecords(int tr)
        {
            this.TotalRecords = tr;
            return this;
        }
    }
    /// <summary>
    /// Message Codes
    /// </summary>
    public enum MessageCodeEnum
    {
        /// <summary>
        /// Undefined
        /// </summary>
        UnDefined = 4000,
        /// <summary>
        /// Email already registered
        /// </summary>
        EmailAlreadyRegistered = 4001
    }
}
