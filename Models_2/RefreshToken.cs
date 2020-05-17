using System;
using System.Collections.Generic;

namespace cw3_apbd.Models_2
{
    public partial class RefreshToken
    {
        public string Id { get; set; }
        public string HashingPassword { get; set; }
        public string Salt { get; set; }
    }
}
