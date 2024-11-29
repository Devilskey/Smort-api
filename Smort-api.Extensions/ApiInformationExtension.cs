using Microsoft.AspNetCore.Builder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Extensions
{
    public static class ApiInformationExtension
    {

        public static void LogApiInfo (this IApplicationBuilder app)
        {
            Log.Information(
                "test"
                );
        }
    }
}
