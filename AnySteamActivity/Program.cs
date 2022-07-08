using Microsoft.Win32;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
using Steam.Models.SteamCommunity;
using System.Net;
using System.Drawing;
using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace AnySteamActivity
{
    internal delegate void SignalHandler(ConsoleSignal consoleSignal);

    internal enum ConsoleSignal
    {
        CtrlC = 0,
        CtrlBreak = 1,
        Close = 2,
        LogOff = 5,
        Shutdown = 6
    }

    internal static class ConsoleHelper
    {
        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        public static extern bool SetSignalHandler(SignalHandler handler, bool add);
    }

    internal class Program
    {
        private static SignalHandler signalHandler;

        private static GlobalSettings globalSettings;

        static long getCurrentUserId()
        {
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Valve\Steam\ActiveProcess"))
            {
                if (key == null) throw new NullReferenceException("Can`t get current steam user through registry");

                object obj = key.GetValue("ActiveUser");
                if (obj == null) throw new NullReferenceException("Can`t get steam user id");

                long id = Convert.ToInt64(obj);
                return id;
            }
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return new Bitmap(imgToResize, size);
        }

        static void Main(string[] args)
        {
            signalHandler += HandleConsoleSignal;
            ConsoleHelper.SetSignalHandler(signalHandler, true);

            Console.WriteLine("Parsing data from file settings.json...");

            globalSettings = JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText("settings.json"));

            DiscordRpcClient client;

            client = new DiscordRpcClient(globalSettings.discord_application_settings.app_id);

            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.Initialize();


            HttpClient http_client = new HttpClient();

            AssetsBuilder.Initialize(globalSettings);

            string prev_game_id = "0";
            DateTime dateTimeStart = DateTime.Now;

            while (true)
            {
                string steam_id;
                try
                {
                    steam_id = getCurrentUserId().ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot get current steam user. Error: " + ex.Message);
                    return;
                }

                string current_user_steam_id = SteamIdConverter.ConvertType(steam_id, SteamIdConverter.IdType.SteamID64);

                SteamWebInterfaceFactory? webInterfaceFactory = new SteamWebInterfaceFactory(globalSettings.steam_api_token);
                if (webInterfaceFactory == null)
                {
                    Console.WriteLine("Invalid api key");
                    return;
                }

                SteamUser? steamInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(http_client);
                ISteamWebResponse<PlayerSummaryModel> playerSummaryResponse = steamInterface.GetPlayerSummaryAsync(Convert.ToUInt64(current_user_steam_id)).Result;
                PlayerSummaryModel playerSummaryData = playerSummaryResponse.Data;

                string game_id = playerSummaryData.PlayingGameId;
                if (game_id == null)
                {
                    client.SetPresence(new RichPresence() { 
                        Details = "Бездействует",
                        Assets = new Assets()
                        {
                            LargeImageKey = globalSettings.discord_application_settings.idling_image,
                        }
                    });
                    prev_game_id = "0";
                    continue;
                }

                if (game_id != prev_game_id)
                {
                    prev_game_id = game_id;
                    dateTimeStart = DateTime.Now;
                }

                OwnedGamesResultModel a = webInterfaceFactory.CreateSteamWebInterface<PlayerService>(http_client).GetOwnedGamesAsync(Convert.ToUInt64(current_user_steam_id), true).Result.Data;
                OwnedGameModel? game = a.OwnedGames.Where(p => p.AppId == Convert.ToUInt32(game_id)).ToList()[0];
                string icon_url = $"http://media.steampowered.com/steamcommunity/public/images/apps/{game.AppId}/{game.ImgIconUrl}.jpg";

                WebClient wc = new WebClient();
                byte[] pic = wc.DownloadData(icon_url);
                Image icon = resizeImage((Bitmap)new ImageConverter().ConvertFrom(pic), new Size(512, 512));

                wc.Dispose();

                if (AssetsBuilder.Assets.Count == 300)
                {
                    foreach (var asset in AssetsBuilder.Assets)
                        AssetsBuilder.DeleteAsset(asset.Key);
                }

                if (!AssetsBuilder.Assets.ContainsKey(game_id)) AssetsBuilder.UploadNewAsset(icon, game_id);

                client.SetPresence(new RichPresence()
                {
                    Details = "Играет в " + game.Name,
                    Assets = new Assets()
                    {
                        LargeImageKey = game_id,
                    },
                    Timestamps = new Timestamps()
                    {
                        StartUnixMilliseconds = (ulong?)((DateTimeOffset)dateTimeStart).ToUnixTimeSeconds()
                    }
                });

                Thread.Sleep(globalSettings.update_delay);
            }
        }

        private static void HandleConsoleSignal(ConsoleSignal consoleSignal)
        {
            globalSettings = new GlobalSettings()
            {
                steam_api_token = globalSettings.steam_api_token,
                update_delay = globalSettings.update_delay,
                discord_application_settings = new DiscordApplicationSettings()
                {
                    app_id = globalSettings.discord_application_settings.app_id,
                    assets = AssetsBuilder.Assets,
                    idling_image = globalSettings.discord_application_settings.idling_image
                },
                discord_request_settings = globalSettings.discord_request_settings
            };

            File.WriteAllText("settings.json", JsonConvert.SerializeObject(globalSettings));
        }
    }
}