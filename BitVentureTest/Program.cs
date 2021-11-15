using BitVentureTest.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace BitVentureTest
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();
        static void Main(string[] args)
        {
            Console.WriteLine("* BASIC ENDPOINT");
            Console.WriteLine("");
            var model = JsonReader("Config/basic_endpoints.json");
            CallService(model);
            Console.WriteLine("_________________________________________________________________________________________________________________________________________");
            Console.WriteLine("");
            Console.WriteLine("* BONUS ENDPOINT");
            Console.WriteLine("");
            var bonusModel = JsonReader("Config/bonus_endpoints.json");
            CallService(bonusModel);
        }

        static ServiceList JsonReader(string path)
        {
            StreamReader r = new StreamReader(path);
            string jsonString = r.ReadToEnd();

            var model = JsonConvert.DeserializeObject<ServiceList>(jsonString);

            return model;
        }


        static void CallService(ServiceList model)
        {
            var countService = 0;
            foreach (var service in model.services)
            {
                countService++;
                var endPoint = 0;
                Console.WriteLine(" ");
                Console.WriteLine("** {1} - SERVICE: {0}", service.baseURL, countService);
                Console.WriteLine(" ");

                if (!service.enabled) continue;

                foreach (var endpoint in service.endpoints)
                {
                    endPoint++;
                    Console.WriteLine("*** {2}.{1} - END POINT: {0} ", endpoint.resource, endPoint, countService);
                    Console.WriteLine(" -------------------------------------------------- ");
                    if (!endpoint.enabled) continue;

                    var response = GetResponse(service.baseURL, endpoint.resource, service.datatype).Result;
                    ReadJsonObject(response, endpoint, service.identifiers);
                    Console.WriteLine("");
                }
               
            }
        }


        static void ReadJsonObject(JObject jobj, EndPoint expectedResponse, List<Identifier> identifiers)
        {
            if (jobj == null) return;
            foreach (var (key, jToken) in jobj)
            {
                switch (jToken)
                {
                    case JObject token:
                        ReadJsonObject(token , expectedResponse, identifiers);
                        break;
                    case JArray array:
                    {
                        var jArray = array;

                        foreach (var token in jArray)
                        {
                            ReadJsonObject(token as JObject, expectedResponse, identifiers);
                        }

                        break;
                    }
                    default:
                        ReadJToken(expectedResponse, identifiers, key, jToken.ToString());
                        break;
                }
            }
        }

        static void ReadJToken(EndPoint expectedResponse, List<Identifier> identifiers, string key, string value)
        {
            expectedResponse.response.ForEach(el =>
            {
                if (!string.IsNullOrEmpty(el.regex))
                {
                    var regex = new Regex(el.regex);
                    if (key == el.element && regex.IsMatch(value))
                    {
                        Console.WriteLine("**** {0} : {1}  matches",key, value);
                    }
                }

                if (!string.IsNullOrEmpty(el.identifier))
                {

                    if (identifiers != null)
                    {

                        identifiers.ForEach(identifier =>
                        {
                            if (identifier.key == el.identifier && key == el.element && value == identifier.value)
                            {
                                Console.WriteLine("**** {0} : {1}  matches", key, value);
                            }
                        });
                    }
                    else
                    if (key == el.element && el.identifier == value)
                    {
                        Console.WriteLine("**** {0} : {1}  matches", key, value);
                    }
                }
            });
        }



        static async Task<JObject> GetResponse(string basedUrl, string endpoint, string datatype)
        {
            JObject responses = null;

            var formatters = new List<MediaTypeFormatter>() {
                new JsonMediaTypeFormatter(),
                new XmlMediaTypeFormatter()
            };
           
            HttpResponseMessage httpResponse = await Client.GetAsync($"{basedUrl}/{endpoint}");
            if (httpResponse.IsSuccessStatusCode)
            {
                if(string.IsNullOrEmpty(datatype) || datatype == "JSON")
                    responses = await httpResponse.Content.ReadAsAsync<JObject>(formatters);
                else if (datatype == "XML")
                {
                    XDocument xdoc = XDocument.Parse(await httpResponse.Content.ReadAsStringAsync());

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xdoc.ToString());
                    string json = JsonConvert.SerializeXmlNode(doc);

                    responses = JObject.Parse(json);

                }

            }
            return responses;
        }

    }
}
