using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CurrentTimeService : ICurrentTimeService
    {
        public DateTime GetCurrentTime() => DateTime.Now;
    }
}
