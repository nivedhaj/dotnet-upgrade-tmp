using ct._Core.Security;
using Microsoft.Extensions.Configuration;
using Octokit;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ct._Core.Service
{
    /// <summary>
    /// Takes care of Github features
    /// </summary>
    public class GithubService
    {
        IConfiguration _configuration { get; }
        IUserContext _uc;
        GitHubClient client;
        string _owner;
        string _repo;
        string _rootFolder;
        string master_branch = "master";
        string const_owner = "Github:Owner";
        string const_repo = "Github:Repo";
        string const_target_folder = "Github:TargetFolder";
        string const_productHeaderValue = "Github:ProductHeaderValue";
        string const_token = "Github:Token";
        string _defaultFolder = "default";

        /// <summary>
        /// Sets up connection with Github
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="uc"></param>
        public GithubService(IConfiguration configuration, IUserContext uc)
        {
            _configuration = configuration;
            _owner = _configuration[const_owner];
            _repo = _configuration[const_repo];
            _rootFolder = _configuration[const_target_folder];

            client = new GitHubClient(new ProductHeaderValue(_configuration[const_productHeaderValue]));
            client.Credentials = new Credentials(_configuration[const_token]);
            _uc = uc.Current();
        }

        /// <summary>
        /// If file is already present updates it, else creates
        /// </summary>
        /// <param name="file_content"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public async Task<RepositoryContentChangeSet> Upsert(string file_content, string file_name)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower();
            var _filePath = Path.Combine(_rootFolder, _folder, file_name);

            RepositoryContent file = null;
            try
            {
                var repository_content = await client.Repository.Content.GetAllContents(_owner, _repo, _filePath);
                file = repository_content.FirstOrDefault(o => o.Name == file_name);
            }
            catch (Octokit.NotFoundException)
            {
                //just ignore 
            }
            if (file == null)
                return await client.Repository.Content.CreateFile(_owner, _repo, _filePath, new CreateFileRequest("File creation", file_content));
            else
                return await client.Repository.Content.UpdateFile(_owner, _repo, _filePath, new UpdateFileRequest("File update", file_content, file.Sha));
        }

        /// <summary>
        /// Downloads the specified file
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public byte[] Download(string file_name)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower();
            var _filePath = Path.Combine(_rootFolder, _folder, file_name);

            var file_content_list = client.Repository.Content.GetAllContentsByRef(_owner, _repo, _filePath, master_branch).Result;
            var file_content = file_content_list.First().Content;
            byte[] file_content_bytes = Encoding.ASCII.GetBytes(file_content);
            return file_content_bytes;
        }

        /// <summary>
        /// Returns the content of the file
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public string GetContent(string file_name)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower();
            var _filePath = Path.Combine(_rootFolder, _folder, file_name);

            var file_content_list = client.Repository.Content.GetAllContentsByRef(_owner, _repo, _filePath, master_branch).Result;
            var file_content = file_content_list.First().Content;
            return file_content.ToString();
        }

        /// <summary>
        /// Return the URL of the specified file
        /// </summary>
        /// <param name="file_name"></param>
        /// <returns></returns>
        public string GetUrl(string file_name)
        {
            var _folder = string.IsNullOrEmpty(_uc.ShortName) ? _defaultFolder : _uc.ShortName.ToLower();
            var _filePath = Path.Combine(_rootFolder, _folder, file_name);

            var file_content_list = client.Repository.Content.GetAllContentsByRef(_owner, _repo, _filePath, master_branch).Result;
            var file_downloadUrl = file_content_list.First().DownloadUrl;
            return file_downloadUrl.ToString();
        }
    }
}
