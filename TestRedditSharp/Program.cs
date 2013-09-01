using System;
using System.Collections.Generic;
using System.Linq;
using RedditSharp;
using System.Security.Authentication;

namespace TestRedditSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var reddit = new Reddit();
            while (reddit.User == null)
            {
                Console.Write("Username: ");
                var username = Console.ReadLine();
                Console.Write("Password: ");
                var password = ReadPassword();
                try
                {
                    Console.WriteLine("Logging in...");
                    reddit.LogIn(username, password);
                }
                catch (AuthenticationException)
                {
                    Console.WriteLine("Incorrect login.");
                }
            }
            var subreddit = reddit.GetSubreddit("pokemon");
            var posts = subreddit.GetNew();
            foreach (var post in posts.Take(25))
                Console.WriteLine("/u/{0}: (+{1}-{2}:{3}) {4}", post.AuthorName, post.Upvotes, post.Downvotes, post.Score, post.Title);
            var moderators = subreddit.GetModerators();
            foreach (var mod in moderators)
                Console.WriteLine("/u/{0} is a moderator of {1} with perms: {2}", mod.Name, subreddit, mod.Permissions);
            Console.ReadKey(true);
        }

        public static string ReadPassword()
        {
            var passbits = new Stack<string>();
            //keep reading
            for (ConsoleKeyInfo cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true))
            {
                if (cki.Key == ConsoleKey.Backspace)
                {
                    //rollback the cursor and write a space so it looks backspaced to the user
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    passbits.Pop();
                }
                else
                {
                    Console.Write("*");
                    passbits.Push(cki.KeyChar.ToString());
                }
            }
            string[] pass = passbits.ToArray();
            Array.Reverse(pass);
            Console.Write(Environment.NewLine);
            return string.Join(string.Empty, pass);
        }
    }
}
