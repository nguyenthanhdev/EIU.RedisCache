using System.Text.RegularExpressions;

namespace EIU.Caching.Redis.Helpers
{
    /// <summary>
    /// Hỗ trợ parse chuỗi thời gian dạng "1d2h30m" → số giây (int)
    /// </summary>
    public static class DurationHelper
    {
        /// <summary>
        /// Parse chuỗi thời gian kiểu "1d2h30m" (day/hour/minute/second) → giây
        /// </summary>
        public static int ParseDuration(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            int totalSeconds = 0;
            var pattern = @"(\d+)([dhms])"; // d=day, h=hour, m=minute, s=second
            var matches = Regex.Matches(text.ToLower(), pattern);

            foreach (Match match in matches)
            {
                if (!int.TryParse(match.Groups[1].Value, out int value))
                    continue;

                string unit = match.Groups[2].Value;

                totalSeconds += unit switch
                {
                    "d" => value * 24 * 60 * 60,
                    "h" => value * 60 * 60,
                    "m" => value * 60,
                    "s" => value,
                    _ => 0
                };
            }

            return totalSeconds;
        }

        /// <summary>
        /// Cho phép nhập int (giây) hoặc string (vd "1d2h") – dùng khi đọc từ config
        /// </summary>
        public static int GetDurationSeconds(object? value)
        {
            return value switch
            {
                null => 0,
                int seconds => seconds,
                string text => ParseDuration(text),
                _ => 0
            };
        }
    }
}
