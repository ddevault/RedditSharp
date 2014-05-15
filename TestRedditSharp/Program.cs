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
            Reddit reddit = null;
            var authenticated = false;
            while (!authenticated)
            {
                Console.Write("OAuth? (y/n) [n]: ");
                var oaChoice = Console.ReadLine();
                if (!string.IsNullOrEmpty(oaChoice) && oaChoice.ToLower()[0] == 'y')
                {
                    Console.Write("OAuth token: ");
                    var token = Console.ReadLine();
                    reddit = new Reddit(token);
                    reddit.InitOrUpdateUser();
                    authenticated = reddit.User != null;
                    if (!authenticated)
                        Console.WriteLine("Invalid token");
                }
                else
                {
                    Console.Write("Username: ");
                    var username = Console.ReadLine();
                    Console.Write("Password: ");
                    var password = ReadPassword();
                    try
                    {
                        Console.WriteLine("Logging in...");
                        reddit = new Reddit(username, password);
                        authenticated = reddit.User != null;
                    }
                    catch (AuthenticationException)
                    {
                        Console.WriteLine("Incorrect login.");
                        authenticated = false;
                    }
                }
            }
            Console.Write("Create post? (y/n) [n]: ");
            var choice = Console.ReadLine();
            if (!string.IsNullOrEmpty(choice) && choice.ToLower()[0] == 'y')
            {
                Console.Write("Type a subreddit name: ");
                var subname = Console.ReadLine();
                var sub = reddit.GetSubreddit(subname);
                Console.WriteLine("Making test post");
                var post = sub.SubmitTextPost("RedditSharp test", "This is a test post sent from RedditSharp");
                Console.WriteLine("Submitted: {0}", post.Url);
            }
            else
            {
                var subreddit = reddit.GetSubreddit("MediaCrush");
                foreach (var post in subreddit.Posts.Take(10))
                    Console.WriteLine("\"{0}\" by {1}", post.Title, post.Author);
            }
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
