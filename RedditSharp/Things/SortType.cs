using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedditSharp.Things
{
   

    

    /// <summary>
    /// one of (relevance, new, hot, top, comments)
    /// </summary>
    public enum SortType
    {
        [EnumString("relevance")]
        Relevance,
        [EnumString("new")]
        New,
        [EnumString("hot")]
        Hot,
        [EnumString("top")]
        Top,
        [EnumString("comments")]
        Comments

    }
}
