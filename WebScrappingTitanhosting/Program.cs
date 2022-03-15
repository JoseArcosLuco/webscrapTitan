using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using HtmlAgilityPack;
using ScrapySharp.Core;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace WebScrappingTitanhosting
{
    class Program
    {
        

            static ScrapingBrowser _scrapingbrowser = new ScrapingBrowser();
            static void Main(string[] args)
            {
               
                Console.WriteLine("Please enter the Keyword :");
                var Keyword = Console.ReadLine();
                var adLinks = GetAdLinks("https:////www.zoominfo.com//companies-search//location-australia--victoria--avondale-heights");
                var lstAdDetails = GetAdDetails(adLinks, Keyword);
                exportAdsToCsv(lstAdDetails, Keyword);
            }

            static List<string> GetAdLinks(string url)
            {
                var mainPageAdLinks = new List<string>();
                var html = GetHtml(url);
                var links = html.CssSelect("a");
                //var links = html.CssSelect("main");

            foreach (var link in links)
             {
                    try
                    {
                        HtmlAttribute valueAttribute = link.Attributes["class"];

                        if (valueAttribute != null)
                        {
                            if (link.Attributes["href"].Value.Contains("/c/"))
                            {
                                mainPageAdLinks.Add(link.Attributes["href"].Value);
                            }
                        }
                    }
                    
                    catch (Exception ex)
                    {
                        Console.WriteLine("Err: " + ex.Message.ToString());

                    }
                    
                }
                return mainPageAdLinks;
            }

            static List<AdDetails> GetAdDetails(List<string> urls, string Keyword)
            {
                var lstAdDetails = new List<AdDetails>();

                foreach (var url in urls)
                {
                    var htmlNode = GetHtml(url);
                    var AdDetails = new AdDetails();

                    AdDetails.AdTitle = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("//html/head/title").InnerText;
                    var description = htmlNode.OwnerDocument.DocumentNode.SelectSingleNode("///html/body/div").InnerText;
                    var result = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//span[@class='entry-content']").Select(p => p.InnerText).ToList();

                    //var descriptionOut = "";
                    //foreach (var i in result)
                    //{

                    //descriptionOut = i.Contains("h1").ToString();
                        
                    //}
               



                    AdDetails.AdDescription = description.Replace("\n        \n            QR Code Link to This Post\n            \n        \n", "");
                    AdDetails.AdUrl = url;

                    var KeywordInTitle = AdDetails.AdTitle.ToLower().Contains(Keyword.ToLower());
                    var KeywordInDescription = AdDetails.AdDescription.ToLower().Contains(Keyword.ToLower());

                    if (KeywordInTitle || KeywordInDescription)
                    {
                        lstAdDetails.Add(AdDetails);

                    }

                }
                return lstAdDetails;
            }

            static void exportAdsToCsv(List<AdDetails> lstAdDetails, string Keyword)
            {
                using (var writer = new StreamWriter($@"c:/temp/ScrapySharp_scraper/CSVs/{Keyword}_{DateTime.Now.ToFileTime()}.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(lstAdDetails);
                }
            }

            static HtmlNode GetHtml(string url)
            {
                WebPage webPage = _scrapingbrowser.NavigateToPage(new Uri(url));
                return webPage.Html;
            }



        

            public class AdDetails
            {
                public string AdTitle { get; set; }
                public string AdDescription { get; set; }
                public string AdUrl { get; set; }
            }
    }
}
