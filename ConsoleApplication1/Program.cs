using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("testing");
            var reddit = new Reddit();
            var user = reddit.LogIn("******", "******");
            var subreddit = reddit.GetSubreddit("/r/gameofthrones");
            subreddit.Subscribe();
            foreach (var post in subreddit.New.Take(25))
            {
                Console.WriteLine(post.Title);
            }

            Console.ReadLine();
        }
    }
}
