using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Domain;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Exceptions;
using Tweetinvi.Logic.Model;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Tweetinvi.Parameters;


namespace OnTrace.Channel.Infrastructure.Services
{
    public class TwitterHelper
    {
        private readonly FileProcessor _fileProcessor;
        private readonly TwitterAccount _account;

        public TwitterHelper(TwitterAccount account, FileProcessor fileProcessor)
        {
            _fileProcessor = fileProcessor;
            _account = account;

            Auth.SetUserCredentials(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret);
            
            /*
            TweetinviEvents.QueryBeforeExecute += (sender, args) =>
            {
                Console.WriteLine(args.QueryURL);
            };*/
        }

        public string MediaStorageLocationPath { get; set; }

        public void PublishTweet(string text)
        {
            try
            {
                Tweet.PublishTweet(text, new PublishTweetOptionalParameters());
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet..", ex);
            }

        }

        public void PublishTweetWithImage(string text, byte[] image)
        {
            try
            {
                Tweet.PublishTweetWithImage(text, image);
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with image..", ex);
            }
        }

        public void PublishTweetWithImage(string text, List<byte[]> images)
        {
            try
            {
                Tweet.PublishTweet(text, new PublishTweetOptionalParameters()
                {
                   MediaBinaries = images
                });
                
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with image..", ex);
            }
        }


        public void PublishTweetWithImage(string text, string imagePath)
        {
            try
            {
                var image = _fileProcessor.StreamToBytes(imagePath);
                Tweet.PublishTweetWithImage(text, image);
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with image..", ex);
            }
        }

        /// <summary>
        /// Publish tweet with multiple images
        /// </summary>
        /// <param name="text">Tweet Message</param>
        /// <param name="images">List of images path</param>
        public void PublishTweetWithImage(string text, List<string> images)
        {
            try
            {
                List<byte[]> medias = images.Select(image => _fileProcessor.StreamToBytes(image)).ToList();

                Tweet.PublishTweet(text, new PublishTweetOptionalParameters()
                {
                    MediaBinaries = medias
                });
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with images..", ex);
            }
        }


        public void PublishTweetWithVideo(string text, byte[] video)
        {
            try
            {
                Tweet.PublishTweetWithVideo(text, video);
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with video..", ex);
            }
        }

        public void PublishTweetWithVideo(string text, string videoPath)
        {
            try
            {
                var video = _fileProcessor.StreamToBytes(videoPath);
                var media = Upload.UploadVideo(video);
                
                Tweet.PublishTweet(text, new PublishTweetOptionalParameters()
                {
                    Medias = new List<IMedia>() { media }
                });
            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
               throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish tweet with video", ex);
            }
        }


        public void PublishMessage(string message, string to)
        {
            try
            {
                var recipient = User.GetUserFromScreenName(to);
                Message.PublishMessage(message, recipient);

            }
            catch (ArgumentException ex)
            {
                //Something went wrong with the arguments and request was not performed
                throw new ArgumentException("Request parameters are invalid.", ex);
            }
            catch (TwitterException ex)
            {
                throw new Exception($"Something went wrong when we tried to execute the http request: {ex.TwitterDescription}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to publish message to {to}", ex);
            }
        }

        public List<TwitterTweet> GetMentionsTimeline(DateTime since, DateTime until, int maxTweet)
        {
            try
            {
                var param = new SearchTweetsParameters(_account.Username)
                {
                    Since = since,
                    Until = until,
                    MaximumNumberOfResults = maxTweet
                };

                var tweetIds = Search.SearchTweets(param).Select(x => x.Id);
                var tweets = Tweet.GetTweets(tweetIds.ToArray());
                List<TwitterTweet> result = new List<TwitterTweet>();

                foreach (ITweet tweet in tweets)
                {
                    var media = GetTweetMedia(tweet);
                    var item = new TwitterTweet()
                    {
                        Id = tweet.Id,
                        CreatedBy = tweet.CreatedBy.ScreenName,
                        CreatedAt = tweet.CreatedAt,
                        Text = Regex.Replace(tweet.FullText, @"http[^\s]+", ""),
                        Media = media,
                        Type = media.Any() ? 1 : 0
                    };

                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve mentions timeline.", ex);
            }
        }

        public List<TwitterTweet> GetMentionsTimeline(DateTime since, DateTime until)
        {
            return GetMentionsTimeline(since, until, 100);
        }

        public List<TweetMedia> GetTweetMedia(ITweet tweet)
        {
            try
            {
                var result = new List<TweetMedia>();
                foreach (var entity in tweet.Media)
                {
                    string mediaUrl = entity.MediaURL;
                    if (entity.MediaType != "photo") mediaUrl = entity.VideoDetails.Variants[0].URL;

                    string pathToWrite = MediaStorageLocationPath + _fileProcessor.GetFilename(mediaUrl);
                    _fileProcessor.DownloadAndWriteMedia(mediaUrl, pathToWrite );

                    var media = new TweetMedia()
                    {
                        Filename = _fileProcessor.GetFilename(mediaUrl),
                        Type = _fileProcessor.GetExtension(mediaUrl),
                        Url = mediaUrl,
                        Filedata = _fileProcessor.StreamToBytes(pathToWrite)
                    };

                    result.Add(media);
                    
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get tweet media. [id:{tweet.Id}]", ex);
            }
        }

        public List<TwitterTweet> SearchMentionsTimeline(DateTime since, DateTime until, int maxTweet)
        {
            try
            {
                var param = new SearchTweetsParameters(_account.Username)
                {
                    Since = since,
                    Until = until,
                    MaximumNumberOfResults = maxTweet
                };
                
                var tweetIds = Search.SearchTweets(param).Select(x => x.Id).ToArray();
                var tweets = Tweet.GetTweets(tweetIds);

                List<TwitterTweet> result = new List<TwitterTweet>();

                foreach (ITweet tweet in tweets)
                {
                    List<TweetMedia> mediaList = new List<TweetMedia>();
                    foreach (var entity in tweet.Media)
                    {
                        string mediaUrl = entity.MediaURL;
                        if (entity.MediaType != "photo") mediaUrl = entity.VideoDetails.Variants[0].URL;

                        var media = new TweetMedia()
                        {
                            Filename = _fileProcessor.GetFilename(mediaUrl),
                            Type = _fileProcessor.GetExtension(mediaUrl),
                            Url = mediaUrl
                        };

                        mediaList.Add(media);
                    }

                    var item = new TwitterTweet()
                    {
                        Id = tweet.Id,
                        CreatedBy = "@" + tweet.CreatedBy.ScreenName,
                        CreatedAt = tweet.CreatedAt,
                        Text = Regex.Replace(tweet.FullText, @"http[^\s]+", ""),
                        Media = mediaList,
                        Type = mediaList.Any() ? 1 : 0
                    };

                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve mentions timeline.", ex);
            }
        }
        

        public IEnumerable<TwitterMessage> GetPrivateMessages()
        {
            var messages = Message.GetLatestMessagesReceived();

            if (messages != null && messages.Any())
            {

                return messages.Select(message => new TwitterMessage()
                {
                    Id = message.Id,
                    Sender = message.SenderScreenName,
                    Recipient = message.RecipientScreenName,
                    CreatedAt = message.CreatedAt,
                    Text = message.Text,
                    Media = new List<TweetMedia>()
                });
            }
            else
            {
                return Enumerable.Empty<TwitterMessage>();
            }
        }

        public void DestroyMessage(long messageId)
        {
            Message.DestroyMessage(messageId);
        }

        public void SearchTimeline(DateTime since, DateTime until)
        {
            var param = new SearchTweetsParameters(_account.Username)
            {
                Since = since,
                Until = until,
                MaximumNumberOfResults = 100
            };
            
            Console.WriteLine($"Search tweet between {since} and {until}");

            var tweetIds = Search.SearchTweets(param).Select(x => x.Id);
            
            foreach (long id in tweetIds)
            {
                Console.WriteLine($"tweetid : {id}");
            }

            
            Console.WriteLine("---------------------------");

           
            var tweets = Tweet.GetTweets(tweetIds.ToArray());
            foreach (var tweet in tweets)
            {
                Console.WriteLine($"id : {tweet.Id} - text : {tweet.FullText} - createdAt : {tweet.CreatedAt}");
            }
        }
        
    }
}
