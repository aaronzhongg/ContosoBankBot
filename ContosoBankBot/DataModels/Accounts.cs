using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot.DataModels
{
    public class Accounts
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "USERNAME")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "PASSWORD")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "PERMISSIONS")]
        public string Permissions { get; set; }
    }
}