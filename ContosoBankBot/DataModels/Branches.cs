using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoBankBot.Models
{
    public class Branches
    {   
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "NAME")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "LOCATION")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "WEEKDAY_OPENING")]
        public string WeekdayOpen { get; set; }

        [JsonProperty(PropertyName = "WEEKDAY_CLOSING")]
        public string WeekdayClose { get; set; }

        [JsonProperty(PropertyName = "WEEKEND_OPENING")]
        public string WeekendOpen { get; set; }

        [JsonProperty(PropertyName = "WEEKEND_CLOSING")]
        public string WeekendClose { get; set; }

        [JsonProperty(PropertyName = "MANAGER")]
        public string Manager { get; set; }
    }
}