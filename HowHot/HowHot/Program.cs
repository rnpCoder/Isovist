using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HowHot
{
    class Program
    {
        static WebClient wc = new WebClient();


        static void Main(string[] args)
        {
            if (args != null)
            {
                Console.WriteLine("");
                List<string> argStrings = args.ToList();
                if (argStrings.Count > 0)

                {
                    if (argStrings[0] == "help")
                    {
                        Console.WriteLine("The temperature is reported by default for a single location as set in the config file.");
                        Console.WriteLine("The temperature can optionally be listed for multiple locations. Use the parameter of multi.");
                        Console.WriteLine("The command line is:");
                        Console.WriteLine("HowHot multi");
                        Console.WriteLine("Location ids are configured in the config file. A full list is available at http://bulk.openweathermap.org/sample/city.list.json.gz or https://dl.dropboxusercontent.com/u/24636206/isovist/city.list.json");
                    }

                    if (argStrings[0] == "multi")
                    {
                        FetchTemperatureMulti();
                    }
                }
                else
                {
                    ReportTemperature();
                }
            }
        }



        private static void ReportTemperature()
        {
            try
            {
                string tempUnits = "metric";
                String owmReqUri = String.Format("http://api.openweathermap.org/data/2.5/weather?id={0}&appid={1}&units={2}&mode=xml",
                                              HowHot.Properties.Settings.Default.SingleCityId,
                                              HowHot.Properties.Settings.Default.OwmApiKey,
                                              tempUnits);
                Uri uriWeatherSource = new Uri(owmReqUri);

                string owmResponse = wc.DownloadString(uriWeatherSource);
                if (!String.IsNullOrEmpty(owmResponse))
                {
                    XmlDocument srcXml = new XmlDocument();
                    srcXml.LoadXml(owmResponse);
                    WriteTemperatureToConsole(srcXml.SelectNodes("//city")[0].Attributes.GetNamedItem("name").Value,
                                                srcXml.SelectNodes("//temperature")[0].Attributes.GetNamedItem("value").Value);
                }
            }
            catch (Exception ex)
            {
                WriteProblemToConsole("Single temperature fetch exception", ex);
            }
        }

        


        private static void FetchTemperatureMulti()
        {
            try
            {
                String owmReqUri = String.Format("http://api.openweathermap.org/data/2.5/group?id={0}&appid={1}",
                                                       HowHot.Properties.Settings.Default.MultiCityIds,
                                                       HowHot.Properties.Settings.Default.OwmApiKey);
                Uri uriWeatherSource = new Uri(owmReqUri);

                string owmResponse = wc.DownloadString(uriWeatherSource);
                if (!String.IsNullOrEmpty(owmResponse))
                {
                    dynamic dyn = JObject.Parse(owmResponse);
                    dynamic cities = dyn.list;
                    for (int i = 0; i < cities.Count; i++)
                    {
                        dynamic city = cities[i];
                        WriteTemperatureToConsole(city.name.Value, KelvinToCelcius(city.main.temp.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteProblemToConsole("Multi temperature fetch exception", ex);
            }

        }
        


        static string KelvinToCelcius(string degKelvin)
        {
            return (Convert.ToDouble(degKelvin) - 273.15).ToString();
        }




        private static void WriteTemperatureToConsole(string location, string temperature)
        {
            Console.WriteLine("{0}: {1} Celcius", location, temperature);
        }


        private static void WriteProblemToConsole(string fetchType, Exception ex)
        {
            Console.WriteLine("A problem occured during a " + fetchType + ".");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("");
        }
    }
}
