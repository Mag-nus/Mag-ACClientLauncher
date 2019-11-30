using System;
using System.Windows.Media;

namespace Mag_ACClientLauncher.ServerManagement
{
    public class ServerItem
    {
        public Guid id { get; set; }

        public string name { get; set; }
        public string description { get; set; }

        public string emu { get; set; }

        public string server_host { get; set; }
        public ushort server_port { get; set; }

        public string type { get; set; }

        public string status { get; set; }

        public string website_url { get; set; }
        public string discord_url { get; set; }

        public override string ToString()
        {
            return $"{name} - {emu}, {server_host}:{server_port}";
        }


        // MVVM stuff for MainWindow.lstPublicServers
        public string Action { get; set; }
        public Brush ActionBackground
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Action))
                    return Brushes.Transparent;

                if (Action == "Import")
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

                return Brushes.LightGreen;
            }
        }
        public string ServerHostAndPort => server_host + ":" + server_port;
        public string ExtendedDescription
        {
            get
            {
                var result = description;

                if (!String.IsNullOrWhiteSpace(website_url))
                {
                    if (!String.IsNullOrWhiteSpace(result))
                        result += Environment.NewLine;
                    result += website_url;
                }

                if (!String.IsNullOrWhiteSpace(discord_url))
                {
                    if (!String.IsNullOrWhiteSpace(result))
                        result += Environment.NewLine;
                    result += discord_url;
                }

                return result;
            }
        }
    }
}
