using System;

namespace RedditSharp
{
    public static class HttpWebReqProxy
    {
        static bool proxyRequired = false;
        static string proxyAddress = null;
        static string proxyUsername = null;
        static string proxyPassword = null;

        public static void SetupWebRequestProxy(string proxAddress, string proxUsername, string proxPassword)
        {
            proxyRequired = true;
            proxyAddress = proxAddress;
            proxyUsername = proxUsername;
            proxyPassword = proxPassword;
        }

        public static void DisableWebRequestProxy()
        {
            proxyRequired = false;
            proxyAddress = null;
            proxyUsername = null;
            proxyPassword = null;
        }


        public static void InitWebReqProxy(this System.Net.HttpWebRequest req)
        {
            if (!proxyRequired) { return; }

            System.Net.WebProxy myProxy = new System.Net.WebProxy();

            myProxy.Address = new Uri(proxyAddress);
            myProxy.Credentials = new System.Net.NetworkCredential(proxyUsername, proxyPassword);

            req.Proxy = myProxy;
        }

    }
}
