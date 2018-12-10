using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace AlarmServer.Core
{
    public class AlarmProfile
    {
        [XmlAttribute("name")]
        public string ProfileName { get; set; }
        [XmlAttribute("id")]
        public string ProfileId { get; set; }
        [XmlAttribute("target")]
        public string ProfileTarget { get; set; }
        [XmlAttribute("agent")]
        public string RequestAgent { get; set; }

        public string Call()
        {
            //Skip SSL Certificate Validation
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string matchPattern = @"\bauth\b\((.*):(.*)\)@(.*)";
            string username = "";
            string password = "";
            string url = "";

            bool auth = false;

            if (Regex.IsMatch(ProfileTarget, matchPattern))
            {
                //Request with Authentification
                var match = Regex.Match(ProfileTarget, matchPattern);

                username = match.Groups[1].Value;
                password = match.Groups[2].Value;
                url = match.Groups[3].Value;

                auth = true;
            }
            else
            {
                //Request without Authentification
                url = ProfileTarget;
            }
            HttpWebRequest webRequest = WebRequest.CreateHttp(url);

            if (auth)
                webRequest.Credentials = new NetworkCredential(username, password);

            webRequest.UserAgent = RequestAgent;
            webRequest.Method = "GET";

            var response = webRequest.GetResponse();

            using (response)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                return reader.ReadToEnd();
            }
        }

    }
}
