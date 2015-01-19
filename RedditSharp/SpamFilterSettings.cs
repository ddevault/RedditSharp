using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditSharp
{
    public class SpamFilterSettings
    {
        public SpamFilterStrength LinkPostStrength { get; set; }
        public SpamFilterStrength SelfPostStrength { get; set; }
        public SpamFilterStrength CommentStrength { get; set; }

        public SpamFilterSettings()
        {
            LinkPostStrength = SpamFilterStrength.High;
            SelfPostStrength = SpamFilterStrength.High;
            CommentStrength = SpamFilterStrength.High;
        }
    }
}
