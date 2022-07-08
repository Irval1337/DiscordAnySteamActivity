namespace AnySteamActivity
{
    internal class GlobalSettings
    {
        public string steam_api_token { get; set; }
        public DiscordRequestSettings discord_request_settings { get; set; }
        public DiscordApplicationSettings discord_application_settings { get; set; }
        public int update_delay { get; set; }
    }

    internal class DiscordRequestSettings
    {
        public string authorization { get; set; }
        public string cookie { get; set; }
        public string user_agent { get; set; }
        public string sec_ch_ua { get; set; }
        public string sec_ch_ua_mobile { get; set; }
        public string sec_ch_ua_platform { get; set; }
        public string sec_ch_ua_dest { get; set; }
        public string sec_ch_ua_mode { get; set; }
        public string sec_ch_ua_site { get; set; }
    }

    internal class DiscordApplicationSettings
    {
        public string app_id { get; set; }
        public Dictionary<string, string>? assets { get; set; }
        public string idling_image { get; set; }
    }
}
