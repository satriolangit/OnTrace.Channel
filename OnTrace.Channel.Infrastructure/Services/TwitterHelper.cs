using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Authentication;



namespace OnTrace.Channel.Infrastructure.Services
{
    public class TwitterHelper
    {
        public void TweetTest()
        {
            string CONSUMER_KEY = "hlh39PtlVTlEYAI1VocC0SZRK";
            string CONSUMER_SECRET = "R3BSTRLSEBKftd2yRklV10QzVUePnOHxFAJCJa21p3CeXQDt3D";
            string ACCESS_TOKEN = "oXjmGlVrghFGdQzgtQzwicDft389qLi";
            string ACCESS_TOKEN_SECRET = "PK62Ux0YX7YqS4PlzrvPLs5foaunmmckVkK40sINczAAB";

            TwitterCredentials cred = new TwitterCredentials(CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET);


            Auth.SetUserCredentials(CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN, ACCESS_TOKEN_SECRET);

            //Auth.ApplicationCredentials = new TwitterCredentials(CONSUMER_KEY, CONSUMER_SECRET, ACCESS_TOKEN,
            //    ACCESS_TOKEN_SECRET);

            Tweet.PublishTweet("Hello, first tweet via ontrace channel..horaaay.!!");
            var authenticatedUser = User.GetAuthenticatedUser();

            Console.WriteLine("test twitter");

        }
    }
}
