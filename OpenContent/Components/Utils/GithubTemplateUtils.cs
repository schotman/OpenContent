using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Satrabel.OpenContent.Components
{
    public static class GithubTemplateUtils
    {
        // 1. api call to template list: https://api.github.com/repos/sachatrauwaen/OpenContent-Templates/contents (type='dir')

        // 2. per template ophalen manifest: 
        // 3. uit manifest (master branch) de naam, onmschrijving en evt. afbeelding lezen 
        // inhoud schema file(is json):
        // https://raw.githubusercontent.com/sachatrauwaen/OpenContent-Templates/master/Bootstrap3Accordion/manifest.json

        // 4. lijst templates opbouwen voor tonen

        // vervolgens:
        // 5. template laten kiezen
        // 6. om eigen/locale template naam vragen plus locatie (radio button)? host map /skin map / portal map

        // 7. alle files uit het gekozen template downloaden naar locale template map
        // JSON lijst van files in OC template, voor download in locale map
        // https://api.github.com/repos/sachatrauwaen/OpenContent-Templates/contents/Bootstrap3Accordion



        // Git templates
        public static JArray GetTemplateList()
        {
            // we need to force the protocol to TLS 1.2
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            JArray content = null;
            string url = "https://api.github.com/repos/sachatrauwaen/OpenContent-Templates/contents";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            var response = client.GetStringAsync(new Uri(url)).Result;

            if (response != null)
            {
                content = JArray.Parse(response);
            }
            return content;

        }
        // all registed github templates (datasource for the repeater)
        public static List<string> ProcessGithubTemplates()
        {
            List<string> templatelist = new List<string>();
            JArray templates = GetTemplateList();
            foreach (JObject t in templates)
            {
                dynamic template = t;
                string name = template.name;

                if (template.type =="dir")
                {
                    JObject manifestfile = GetManifestFile(name);
                    if (manifestfile != null)
                    {
                        dynamic manifest = manifestfile;
                        string item = "<div class='templateitem'>";

                        string title = manifest.Title;
                        if (title != "") { item = item + "<span class='templatetitle'>" + title + "</span>"; }
                        else { item = item + "<span class='templatetitle'>" + name + "</span>"; }

                        string imageurl = "https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/" + name + "/" + manifest.Image;
                        if (imageurl != "") { item = item + "<img class='templateimage' src='" + imageurl + "'/>"; }

                        string description = manifest.Description;
                        if (description != "") { item = item + "<span class='templatedescription'>" + description + "</span>"; }

                        item = item + "</div>";

                        templatelist.Add(item);
                    }
                    else
                    {
                        templatelist.Add("<div class='templateitem'><span class='templatetitle'>" + name + "</span></div>");
                    }
                }

            }

            return templatelist;
        }


        public static JObject GetManifestFile(string templatename)
        {
            JObject manifest = null;
            string manfesturl = "https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/" + templatename + "/manifest.json";

            //  "https://raw.githubusercontent.com/sachatrauwaen/OpenContent-Templates/master/" + tempatename + "/manifest.json";
            // https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/Bootstrap3Columns/manifest.json

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            Uri uri = new Uri(manfesturl);
            Task<HttpResponseMessage> getManifest = client.GetAsync(uri);
            getManifest.Wait();
            var response = getManifest.Result;
    
            if (response.IsSuccessStatusCode)
            {
                Task<string> content = response.Content.ReadAsStringAsync(); 
                content.Wait();
                var c1 = content.Result;
                manifest = JObject.Parse(c1);
            }
            return manifest;
        }

        public static string getFileContent(string templatename, string filename)
        {
            string filecontent = "";
            string fileurl = "https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/" + templatename + filename;
            //  "https://raw.githubusercontent.com/sachatrauwaen/OpenContent-Templates/master/"

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            Uri uri = new Uri(fileurl);
            Task<HttpResponseMessage> getfilecontent = client.GetAsync(uri);
            getfilecontent.Wait();
            var response = getfilecontent.Result;

            if (response.IsSuccessStatusCode)
            {
                Task<string> content = response.Content.ReadAsStringAsync();
                content.Wait();
                filecontent = content.Result;
            }

            return filecontent;
        }



    }
}