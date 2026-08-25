using Antlr4.StringTemplate;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace ct._Core.Service
{
    /// <summary>
    /// Service to get templates
    /// </summary>
    public class TemplateService
    {
        private readonly GithubService github_service;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="githubService"></param>
        public TemplateService(GithubService githubService)
        {
            github_service = githubService;
        }
        /// <summary>
        /// Get template
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public string GetTemplate([CallerMemberName] string methodName = "", string suffix = ""){
            if (methodName.StartsWith("Get", StringComparison.InvariantCultureIgnoreCase)) methodName = methodName.Substring(3);
            var fn = string.IsNullOrEmpty(suffix) ? "_Content.st" : $"_Content_{suffix}.st";
            var template_name =  methodName + fn;
            var template = github_service.GetContent($"template/{template_name}");
            return template;
        }
        /// <summary>
        /// Get Model from Github for testing
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public string GetModel([CallerMemberName] string methodName = "", string suffix = ""){
            if (methodName.StartsWith("Get", StringComparison.InvariantCultureIgnoreCase)) methodName = methodName.Substring(3);
            var fn = string.IsNullOrEmpty(suffix) ? "_Model.json" : $"_Model_{suffix}.json";
            var model_name =  methodName + fn;
            var model_json = github_service.GetContent($"model/{model_name}");
            return model_json;
        }
        public string Render(string template_string, string model)
        {
            Template template = GetTemplateInstance(template_string);
            //model
            var obj = JsonHelper.Deserialize(model);
            //render
            template.Add("m", obj);
            var result = template.Render();
            return result;
        }

        /// <summary>
        /// Get template instance
        /// </summary>
        /// <param name="template_string"></param>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        public static Template GetTemplateInstance(string template_string, string instanceName = "rptc")
        {
            if (string.IsNullOrEmpty(template_string)) return null;

            string _template = "";
            _template = template_string.Trim();
            _template = _template.StartsWith(instanceName) ? _template : $"{instanceName}(m)::=<%{_template}%>";

            var t = new TemplateGroupString(string.Empty, _template, '$', '$');
            var template = t.GetInstanceOf(instanceName);
            return template;
        }
    }
}