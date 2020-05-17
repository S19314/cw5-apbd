using System;
using System.Collections.Generic;

namespace cw3_apbd.Models_2
{
    public partial class Autorization
    {
        public int IdUsers { get; set; }
        public string Ip { get; set; }
        public string Date { get; set; }
        public string Request { get; set; }
    }
}
