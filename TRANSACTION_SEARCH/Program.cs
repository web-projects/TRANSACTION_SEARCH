﻿using FILE_SORT.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace APP_CONFIG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            IConfiguration configuration = ConfigurationLoad();

            List<Tuple<string, string, string>> transactionSearch = LoadFileToSort(configuration);
            TransactionSearch.SearchTransaction(transactionSearch);
        }

        static IConfiguration ConfigurationLoad()
        {
            // Get appsettings.json config.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }

        static List<Tuple<string, string, string>> LoadFileToSort(IConfiguration configuration)
        {
            var filePayload = configuration.GetSection("FileGroup")
                     .GetChildren()
                     .ToList()
                     .Select(x => new
                     {
                         fileIn = x.GetValue<string>("Input"),
                         Guid = x.GetValue<string>("Guid"),
                         Request = x.GetValue<string>("Request")
                     });

            List<Tuple<string, string, string>> results = new List<Tuple<string, string, string>>();

            if (filePayload.Count() > 0)
            {
                int index = 0;
                List<string> fileIn = new List<string>();
                fileIn.AddRange(from value in filePayload
                                select filePayload.ElementAt(index++).fileIn);
                index = 0;
                List<string> guid = new List<string>();
                guid.AddRange(from value in filePayload
                              select filePayload.ElementAt(index++).Guid);

                index = 0;
                List<string> request = new List<string>();
                request.AddRange(from value in filePayload
                                 select filePayload.ElementAt(index++).Request);

                foreach (var combinedOutput in fileIn
                    .Zip(guid, (vc, vv) => Tuple.Create(vc, vv))
                    .Zip(request, (vcvv, o) => Tuple.Create(vcvv.Item1, vcvv.Item2, o)))
                {
                    results.Add(combinedOutput);
                }
            }

            return results;
        }
    }
}
