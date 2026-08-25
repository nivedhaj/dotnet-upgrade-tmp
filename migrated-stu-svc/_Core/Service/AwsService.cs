using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ct._Core.Security;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ct._Core.Service
{
    /// <summary>
    /// Takes care of AWS S3 features
    /// </summary>
    public class AWSService
    {
        IUserContext _uc;
        string _access_key;
        string _secret;
        string _bucket;
        string _regionEndPoint;
        string const_aws_access_key = "AWS:access_key";
        string const_aws_secret = "AWS:secret";
        string const_aws_bucket = "AWS:bucket";
        string const_aws_regionEndPoint = "AWS:regionEndPoint";
        string aws_file_path = "Data Templates";
        string _defaultFolder = "Default";

        IConfiguration _configuration { get; }
        AmazonS3Client aws3 = null;
        RegionEndpoint RegionEndpoint = null;

        /// <summary>
        /// Sets up connection to S3
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="uc"></param>
        public AWSService(IConfiguration configuration, IUserContext uc)
        {
            _configuration = configuration;
            _access_key = _configuration[const_aws_access_key];
            _secret = _configuration[const_aws_secret];
            _bucket = _configuration[const_aws_bucket];
            _regionEndPoint = _configuration[const_aws_regionEndPoint];

            if (_regionEndPoint == "APSouth1") RegionEndpoint = RegionEndpoint.APSouth1;
            aws3 = new AmazonS3Client(_access_key, _secret, RegionEndpoint);
            _uc = uc.Current();
        }

        /// <summary>
        /// Uploads specified file to S3
        /// </summary>
        /// <param name="file_name"></param>
        /// <param name="file_content"></param>
        public string Upload(string file_name, Stream file_content)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower();
            var _filePath = Path.Combine(_folder, file_name).ToLower().Replace("\\", "/");

            var r = aws3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = _filePath,
                InputStream = file_content,
            }).Result;

            var url = aws3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = _filePath,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now.AddDays(1)
            });
            return url;
        }

        /// <summary>
        /// Returns the URL of the specified S3 object
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public string GetUrl(string file_name)
        {
            var url = aws3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = aws_file_path + "/" + file_name,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now + TimeSpan.FromMinutes(1)
            });
            return url;
        }

        /// <summary>
        /// Returns the content of the specified S3 object
        /// </summary>
        /// <param name="file_name"></param>
        /// <param name="acadYear"></param>
        /// <param name="ignoreSuffix"></param>
        /// <returns></returns>
        public string GetFileContent(string file_name, string acadYear, bool ignoreSuffix = false)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? 
                            _defaultFolder : ignoreSuffix ? 
                                _uc.ShortName.ToLower() : _uc.ShortName.ToLower() + "-" + acadYear;
            var _filePath = Path.Combine(_folder, file_name).ToLower();
            string content = null;
            var response = aws3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucket,
                Key = _filePath,
            }).Result;
            using (var s = new StreamReader(response.ResponseStream))
            {
                content = s.ReadToEnd();
            }
            return content;
        }

        /// <summary>
        /// Saves CSV as an S3 object with specifed name
        /// </summary>
        /// <param name="csv"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public void saveCSV(string csv, string file_name)
        {
            var r = aws3.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucket,
                Key = aws_file_path + "/" + file_name,
                ContentBody = csv
            }).Result;
        }

        /// <summary>
        /// Remove File from S3
        /// </summary>
        /// <param name="file_name"></param>
        public void RemoveFile(string file_name)
        {
            if (string.IsNullOrEmpty(file_name)) throw new ArgumentNullException(nameof(file_name));
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower() + "-" + "17-18";
            var _filePath = Path.Combine(_folder, file_name);

            var r = aws3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = _filePath,
            }).Result;
        }
    }
}