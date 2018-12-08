using Horoscope.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;

namespace Horoscope
{
    // FeedData
    // Holds info for a single blog feed, including a list of blog posts (FeedItem).
    public class FeedData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }

        private List<FeedItem> _Items = new List<FeedItem>();
        public List<FeedItem> Items
        {
            get
            {
                return this._Items;
            }
        }
    }

    // FeedItem
    // Holds info for a single blog post.
    public class FeedItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime PubDate { get; set; }
        public string Image { get; set; }
        public Uri Link { get; set; }
    }

    // FeedDataSource
    // Holds a collection of blog feeds (FeedData), and contains methods needed to
    // retreive the feeds.
    public class FeedDataSource
    {
        private ObservableCollection<FeedData> _Feeds = new ObservableCollection<FeedData>();
        public ObservableCollection<FeedData> Feeds
        {
            get
            {
                return this._Feeds;
            }
        }

        public async Task GetFeedsAsync()
        {
            DateTime thisDate = DateTime.Now;
            CultureInfo culture = new CultureInfo("en-us");
            string dd = thisDate.ToString("d", culture);

            Task<FeedData> feed1 = 
            
             GetFeedAsync("http://www.findyourfate.com/rss/horoscope-astrology-feed.asp?mode=view&todate="+dd);
            
           this.Feeds.Add(await feed1);

        }
        
        private async Task<FeedData> GetFeedAsync(string feedUriString)
        {
            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri(feedUriString);

            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                // This code is executed after RetrieveFeedAsync returns the SyndicationFeed.
                // Process the feed and copy the data you want into the FeedData and FeedItem classes.
                FeedData feedData = new FeedData();

                if (feed.Title != null && feed.Title.Text != null)
                {
                    feedData.Title = feed.Title.Text;
                }
                if (feed.Subtitle != null && feed.Subtitle.Text != null)
                {
                    feedData.Description = feed.Subtitle.Text;
                }
                if (feed.Items != null && feed.Items.Count > 0)
                {
                    // Use the date of the latest post as the last updated date.
                    //feedData.PubDate = feed.Items[0].PublishedDate.DateTime;

                    foreach (SyndicationItem item in feed.Items)
                    {
                        FeedItem feedItem = new FeedItem();
                        if (item.Title != null && item.Title.Text != null)
                        {
                            feedItem.Title = item.Title.Text;
                            feedItem.Image = feedItem.Title.Substring(0,3);
                             feedItem.Image = "Assets/" + feedItem.Image +".png";
                        }
                        if (item.PublishedDate != null)
                        {
                            feedItem.PubDate = item.PublishedDate.DateTime;
                        }
                        if (item.Authors != null && item.Authors.Count > 0)
                        {
                            feedItem.Author = item.Authors[0].Name.ToString();
                        }

                        if (feed.SourceFormat == SyndicationFormat.Rss20)
                        {
                            if (item.Summary != null && item.Summary.Text != null)
                            {
                                feedItem.Content = item.Summary.Text;
                            }
                            if (item.Links != null && item.Links.Count > 0)
                            {
                                feedItem.Link = item.Links[0].Uri;
                            }
                        }
                        feedData.Items.Add(feedItem);
                    }
                }
                return feedData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Returns the feed that has the specified title.


        
        public static FeedData GetFeed(string title)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as  FeedDataSource;
            try
            {   var matches = _feedDataSource.Feeds.Where((feed) => feed.Title.Equals(title));
                if (matches.Count() == 1) return matches.First();
                return null;
            }
            catch(NullReferenceException)
            {
                var messageDialog = new MessageDialog("An internet connection is needed to download feeds. Please Connect to the internet");
                var result = messageDialog.ShowAsync();
                return null;
            }
            
        }
         // Returns the post that has the specified title.
        public static FeedItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            var _feeds = _feedDataSource.Feeds;
            try
            {
                var matches = _feedDataSource.Feeds.SelectMany(group => group.Items).Where((item) => item.Title.Equals(uniqueId));
                if (matches.Count() == 1) return matches.First();
                return null;
            }
            catch (NullReferenceException)
            { return null; }
       }
    }
}
