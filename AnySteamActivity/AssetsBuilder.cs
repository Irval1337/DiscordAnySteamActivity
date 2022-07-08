using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace AnySteamActivity
{
    internal static class AssetsBuilder
    {
        public static Dictionary<string, string> Assets = new Dictionary<string, string>();
        public static Dictionary<string, string> RequestData = new Dictionary<string, string>(); // add saving

        private static string getBase64String(System.Drawing.Image image)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] imageByte = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));
            return Convert.ToBase64String(imageByte);
        }

        public static void Initialize(GlobalSettings globalSettings)
        {
            RequestData.Add("app_id", globalSettings.discord_application_settings.app_id);

            RequestData.Add("authorization", globalSettings.discord_request_settings.authorization);
            RequestData.Add("cookie", globalSettings.discord_request_settings.cookie);
            RequestData.Add("user-agent", globalSettings.discord_request_settings.user_agent);
            RequestData.Add("sec-ch-ua", globalSettings.discord_request_settings.sec_ch_ua);
            RequestData.Add("sec-ch-ua-mobile", globalSettings.discord_request_settings.sec_ch_ua_mobile);
            RequestData.Add("sec-ch-ua-platform", globalSettings.discord_request_settings.sec_ch_ua_platform);
            RequestData.Add("sec-fetch-dest", globalSettings.discord_request_settings.sec_ch_ua_dest);
            RequestData.Add("sec-fetch-mode", globalSettings.discord_request_settings.sec_ch_ua_mode);
            RequestData.Add("sec-fetch-site", globalSettings.discord_request_settings.sec_ch_ua_site);

            Assets = globalSettings.discord_application_settings.assets ?? new Dictionary<string, string>();
        }

        public static RequestUploadResponse UploadNewAsset(System.Drawing.Image image, string name) // name = app_id
        {
            RestClient client = new RestClient($"https://discord.com");
            RestRequest request = new RestRequest($"/api/v9/oauth2/applications/{RequestData["app_id"]}/assets", Method.Post);

            request.AddHeader("authorization", RequestData["authorization"]);
            request.AddHeader("cookie", RequestData["cookie"]);
            request.AddHeader("user-agent", RequestData["user-agent"]);
            request.AddHeader("sec-ch-ua", RequestData["sec-ch-ua"]);
            request.AddHeader("sec-ch-ua-mobile", RequestData["sec-ch-ua-mobile"]);
            request.AddHeader("sec-ch-ua-platform", RequestData["sec-ch-ua-platform"]);
            request.AddHeader("sec-fetch-dest", RequestData["sec-fetch-dest"]);
            request.AddHeader("sec-fetch-mode", RequestData["sec-fetch-mode"]);
            request.AddHeader("sec-fetch-site", RequestData["sec-fetch-site"]);

            request.AddJsonBody(JsonConvert.SerializeObject(new RequestBody() { image = "data:image/jpeg;base64," + getBase64String(image), name = name, type = "1"}));
            RestResponse? response = client.Execute(request);

            RequestUploadResponse requestResponse = JsonConvert.DeserializeObject<RequestUploadResponse>(response.Content ?? "{}");

            if (requestResponse.id != null) Assets.Add(name, requestResponse.id.ToString());

            return requestResponse;
        }

        public static void DeleteAsset(string name)
        {
            RestClient client = new RestClient($"https://discord.com");
            RestRequest request = new RestRequest($"/api/v9/oauth2/applications/{RequestData["app_id"]}/assets/{Assets[name]}", Method.Delete);

            request.AddHeader("authorization", RequestData["authorization"]);
            request.AddHeader("cookie", RequestData["cookie"]);
            request.AddHeader("user-agent", RequestData["user-agent"]);
            request.AddHeader("sec-ch-ua", RequestData["sec-ch-ua"]);
            request.AddHeader("sec-ch-ua-mobile", RequestData["sec-ch-ua-mobile"]);
            request.AddHeader("sec-ch-ua-platform", RequestData["sec-ch-ua-platform"]);
            request.AddHeader("sec-fetch-dest", RequestData["sec-fetch-dest"]);
            request.AddHeader("sec-fetch-mode", RequestData["sec-fetch-mode"]);
            request.AddHeader("sec-fetch-site", RequestData["sec-fetch-site"]);

            RestResponse? response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) Assets.Remove(name);
        }

        private class RequestBody
        {
            public string? name { get; set; }
            public string? image { get; set; }
            public string? type { get; set; }
        }

        #region JsonClasses
        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }

        public class Errors
        {
            public Image image { get; set; }
            public Type type { get; set; }
        }

        public class Image
        {
            public List<Error> _errors { get; set; }
        }

        public class Type
        {
            public List<Error> _errors { get; set; }
        }
        #endregion
        public class RequestUploadResponse
        {
            public string? id { get; set; }
            public int? type { get; set; }
            public string? name { get; set; }

            public int? code { get; set; }
            public Errors? errors { get; set; }
            public string? message { get; set; }
        }
    }
}
