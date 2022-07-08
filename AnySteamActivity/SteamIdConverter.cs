namespace AnySteamActivity
{
    internal static class SteamIdConverter
    {
        private const long change_val = 76561197960265728;

        private static IdType Recognize_type(string steam_id)
        {
            if (steam_id == null) return IdType.Unknown;

            if (steam_id[0] == 'S') return IdType.SteamID;
            if (new List<char>() { 'U', 'I', 'M', 'G', 'A', 'P', 'C', 'g', 'T', 'L', 'C', 'a' }.Contains(steam_id[0])) return IdType.SteamID3;
            if (steam_id[0] >= '0' && steam_id[0] <= '9' && steam_id.Length < 17) return IdType.SteamID32;
            if (steam_id[0] >= '0' && steam_id[0] <= '9' && steam_id.Length == 17) return IdType.SteamID64;

            return IdType.Unknown;
        }

        public static string ConvertType(string fromStr, IdType toType)
        {
            IdType fromType = Recognize_type(fromStr);

            if (fromType == IdType.Unknown) throw new NullReferenceException("Unknown steam id type");

            string steam_id32 = null;
            long steam_id32_long = 0;

            switch (fromType)
            {
                case IdType.SteamID: // STEAM_0:y:zzzzzz
                    string y = fromStr.Substring(8, 1);
                    string z = fromStr.Substring(10);
                    steam_id32_long = Convert.ToInt64(z) * 2 + Convert.ToInt64(y);
                    steam_id32 = steam_id32_long.ToString();
                    break;
                case IdType.SteamID3:
                    steam_id32 = fromStr.Substring(4);
                    steam_id32_long = Convert.ToInt64(steam_id32);
                    break;
                case IdType.SteamID32:
                    steam_id32 = fromStr; 
                    steam_id32_long = Convert.ToInt64(steam_id32);
                    break;
                case IdType.SteamID64:
                    steam_id32_long = Convert.ToInt64(fromStr) - change_val;
                    steam_id32 = steam_id32_long.ToString();
                    break;
            }

            switch (toType)
            {
                case IdType.SteamID: // STEAM_0:y:zzzzzz
                    return "STEAM_0:" + oddity(steam_id32_long).ToString() + ":" + (steam_id32_long / 2).ToString();
                case IdType.SteamID3:
                    return "U:1:" + steam_id32;
                case IdType.SteamID32:
                    return steam_id32;
                case IdType.SteamID64:
                    return (steam_id32_long + change_val).ToString();
                default:
                    throw new Exception("Incorrect steam id destination type");
            }
        }

        private static long oddity(long num)
        {
            return num % 2;
        }

        internal enum IdType { Unknown, SteamID, SteamID3, SteamID32, SteamID64 };
    }
}
