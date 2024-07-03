using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commons
{
    public class FirebaseConfiguration
    {
        public string ApiKey { get; set; } = default!;
        public string Bucket { get; set; } = default!;
        public string ProjectId { get; set; } = default!;
        public string AuthDomain { get; set; } = default!;
        public string AuthEmail { get; set; } = default!;
        public string AuthPassword { get; set; } = default!;
    }
}
