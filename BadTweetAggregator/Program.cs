using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BadAPIIqvia;
using BadAPIIqvia.Client;
using BadAPIIqvia.Api;
using BadAPIIqvia.Model;
using System.IO;
using Newtonsoft.Json;

namespace BadTweetAggregator
{
    class Program
    {
        /// <summary>
        /// Main entry point for program.
        /// 
        /// Expects two dates in format (DD/MM/YYYY), either through command line or prompted in, and returns all tweets made between those days inclusive
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var apiInstance = new TweetsApi("https://badapi.iqvia.io/");

            DateTime? startDate;
            DateTime? endDate;
            
            // If length 2, dates are passed in
            if (args.Length == 2)
            {
                startDate = DateTime.Parse(args[0]);
                endDate = DateTime.Parse(args[1]);
            }
            else if (args.Length == 0)
            {
                Console.Write("Start Date (DD/MM/YYYY): ");
                startDate = DateTime.Parse(Console.ReadLine());

                Console.Write("End Date   (DD/MM/YYYY): ");
                endDate = DateTime.Parse(Console.ReadLine());

                // Adjustment to include full day
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            } else
            {
                Console.WriteLine("Please input two dates in the format DD/MM/YYYY");
                return;
            }

            Console.Write("\nGathering tweets");

            // Gather the tweets
            List<Tweet> allTweets = new List<Tweet>();
            DateTime? nextDate = startDate;
            try
            {
                List<Tweet> result;
                do
                {
                    result = apiInstance.ApiV1TweetsGet(nextDate, endDate);
                    allTweets.AddRange(result);

                    if (result.Any())
                    {
                        // Start searching for tweets after the latest dated tweet from previous result
                        nextDate = new DateTime?(result.Last().Stamp.Value.AddTicks(1));
                    }

                    Console.Write(".");
                }
                while (result.Count == 100);

                Console.WriteLine("\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when calling TweetsApi.ApiV1TweetsGet: " + e.Message);
            }

            Console.WriteLine("Gathered {0} tweets", allTweets.Count);
            Console.WriteLine("First tweet posted at {0}, last at {1}", allTweets.First().Stamp.Value.ToUniversalTime(), allTweets.Last().Stamp.Value.ToUniversalTime());

            Console.WriteLine("\nWriting to file... ");

            File.WriteAllText("AllTweets.json", JsonConvert.SerializeObject(allTweets));

            Console.WriteLine("Done");

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
