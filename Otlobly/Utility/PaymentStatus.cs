using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otlobly.Utility
{
    public  class PaymentStatus
    {
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusRejected = "Rejected";
        public string Publishablekey { get; set; }
        public  string Secretkey { get; set; }
    }
}
