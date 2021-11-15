using System.Collections.Generic;
using System.Text.Json;

namespace BitVentureTest.Models
{
    public class BvService
    {
        public string baseURL { get; set; }
        public string datatype { get; set; }
        public bool enabled { get; set; }
        public List<EndPoint>endpoints { get; set; }
        public List<Identifier> identifiers { get; set; }

    }

    public class ServiceList
    {
        public List<BvService> services { get; set; }
    }
}