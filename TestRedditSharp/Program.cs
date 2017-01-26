using System;
using System.Collections.Generic;
using System.Linq;
using RedditSharp;
using System.Security.Authentication;
using RedditSharp.Things;

namespace TestRedditSharp
{
    class Program
    {
        static Reddit reddit = null;

        static void Main(string[] args)
        {
            Authenticate();
            Menu();
        }

        private static void Menu()
        {
            var choice = string.Empty;
            while (choice != "x")
            {
                WriteMenu();
                choice = Console.In.ReadLine();
                switch ((choice ?? string.Empty).ToLower())
                {
                    case ("a"):
                        ShowTop10();
                        break;

                    case ("b"):
                        GetRandom();
                        break;
                }
            }
        }

        //choice a
        private static void ShowTop10()
        {
            Console.Write("subreddit name >> ");
            var subname = Console.ReadLine();
            var sub = reddit.GetSubreddit(subname);
            var top10 = sub.GetTop(FromTime.Week).Take(10);
            int postNumber = 0;
            foreach (var post in top10)
            {
                Console.WriteLine($"{++postNumber}) \"{post.Title}\" \n by {post.AuthorName}\n");
            }
        }

        //choice b
        private static void GetRandom()
        {
            Console.Write("subreddit name >> ");
            var subname = Console.ReadLine();
            var sub = reddit.GetSubreddit(subname);
            var randomPost = sub.Random.FirstOrDefault();
            DisplayPost(randomPost);
        }

        private static void DisplayPost(Post p)
        {
            if (p == null)
            {
                Console.WriteLine("(no post returned)");
                return;
            }

            Console.WriteLine($"\"{p.Title}\"");
            Console.WriteLine($"\tby {p.AuthorName}\"");
            Console.WriteLine($"▲{p.Upvotes} ▼{p.Downvotes}");
            if(!p.IsSelfPost) 
                Console.WriteLine($"Link: {p.Url}");
        }

        private static void WriteMenu()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine($"[a] get top 10");
            Console.WriteLine($"[b] get random post");
            Console.WriteLine(string.Empty);
            Console.Out.Write("choose >> ");
        }

        private static void Authenticate()
        {
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
        }

        public static string ReadPassword()
        {
            var passbits = new Stack<string>();
            //keep reading
            for (ConsoleKeyInfo cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true))
            {
                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (passbits.Count() > 0)
                    {
                        //rollback the cursor and write a space so it looks backspaced to the user
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        passbits.Pop();
                    }
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
