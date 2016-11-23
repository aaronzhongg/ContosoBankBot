using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot.DataModels
{
    public class Atm_Machines
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "LOCATION")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "AVAILABLE")]
        public Boolean Available { get; set; }

    }
}