using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Exceptions
{
    public class GeneratePdfReportException : Exception
    {
        public GeneratePdfReportException(string message) : base(message) { }
    }
}
