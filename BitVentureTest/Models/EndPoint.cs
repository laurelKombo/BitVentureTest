using System.Collections.Generic;

namespace BitVentureTest.Models
{
    public class EndPoint
    {
        public bool enabled { get; set; }
        public string resource  { get; set; }
        public List<Response> response { get; set; }
    }
}