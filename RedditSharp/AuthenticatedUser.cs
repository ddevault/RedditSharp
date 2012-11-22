using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public class AuthenticatedUser : RedditUser
    {
        public AuthenticatedUser(JToken json) : base(json)
        {
            Modhash = json["data"]["modhash"].Value<string>();
            HasMail = json["data"]["has_mail"].Value<bool>();
            HasModMail = json["data"]["has_mod_mail"].Value<bool>();
        }

        public string Modhash { get; set; }
        public bool HasMail { get; set; }
        public bool HasModMail { get; set; }
    }
}