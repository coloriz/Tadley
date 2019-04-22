﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;
using CommandLine;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using InsLab.Muse;
using InsLab.Signal;

namespace Tadley
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<RecordOptions, ExtractOptions>(args)
                .MapResult(
                    (RecordOptions opts) => RunRecordAndReturnExitCode(opts),
                    (ExtractOptions opts) => RunExtractAndReturnExitCode(opts),
                    errs => 1);
        }

        static int RunRecordAndReturnExitCode(RecordOptions opts)
        {
            // create Muse instance
            var dataToRead = opts.Data.Aggregate((acc, x) => acc | x);
            var muse = new Muse()
            {
                DataToRead = dataToRead
            };

            // print Muse info
            Console.WriteLine($"Muse Model = {muse.Model}");
            Console.WriteLine($"Muse Name = {muse.Name}");

            // create GSR/PPG sampler instance
            var gsrPpgSampler = new GsrPpgSampler(opts.PortName, 115200, SamplingRate.SR500Hz, SamplingRate.SR500Hz);

            // print GSR/PPG sampler info
            Console.WriteLine($"GSR/PPG Sampler COM port = {gsrPpgSampler.PortName}, baudrate = {gsrPpgSampler.BaudRate}");
            Console.WriteLine($"GSR sampling rate = {gsrPpgSampler.GsrSamplingRate}");
            Console.WriteLine($"PPG sampling rate = {gsrPpgSampler.PpgSamplingRate}");

            var stopwatch = new Stopwatch();
            bool cancelKeyPressed = false;

            Console.WriteLine("Press any key to start recording");
            Console.ReadKey(true);

            muse.StartReading();
            gsrPpgSampler.StartReading();
            stopwatch.Start();

            // wait for a key press to exit
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancelKeyPressed = true;
            };
            Console.WriteLine("Press Ctrl + C to exit");

            while (!cancelKeyPressed)
            {
                Console.Write($"\r{stopwatch.Elapsed.ToString("g")}");
                Thread.Sleep(100);
            }
            Console.WriteLine();

            stopwatch.Stop();
            muse.StopReading();
            gsrPpgSampler.StopReading();

            Console.WriteLine("Saving data...");
            var museJson = JObject.Parse(muse.ConvertDataToJson());
            var gsrPpgJson = JObject.Parse(gsrPpgSampler.ConvertDataToJson());
            museJson.Merge(gsrPpgJson);
            File.WriteAllText(opts.OutputFile, museJson.ToString());

            return 0;
        }

        static int RunExtractAndReturnExitCode(ExtractOptions opts)
        {
            // read data from input file
            var museDataCollection = MuseUtil.ReadMuseData(opts.InputFile);
            var gsrPpgCollection = GsrPpgUtil.ReadGsrPpgData(opts.InputFile);

            // slice data by specified time span
            var museDataFragments = museDataCollection.SelectByTime(opts.TimeOff, opts.Duration);
            var gsrPpgFragments = gsrPpgCollection.SelectByTime(opts.TimeOff, opts.Duration);

            using (var fs = new FileStream(opts.OutputFile, FileMode.Create, FileAccess.Write))
            using (var package = new ExcelPackage(fs))
            {
                // process Muse data
                foreach (var d in museDataFragments)
                {
                    var sheet = package.Workbook.Worksheets.Add(d.Key.ToString());

                    switch (d.Value[0])
                    {
                        case Channel _:
                            var channels = d.Value.ChangeType<Channel>();
                            var query = from c in channels
                                        select new { Timestamp = new TimeSpan(c.Timestamp).ToString("g"), c.TP9, c.AF7, c.AF8, c.TP10 };
                            sheet.Cells[1, 1].LoadFromCollection(query, PrintHeaders: true);
                            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                            var table = sheet.Tables.Add(sheet.Dimension, d.Key.ToString());
                            // Set table properties (order matters!)
                            table.ShowRowStripes = false;
                            table.TableStyle = TableStyles.Medium9;
                            table.ShowFirstColumn = true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                // process GSR/PPG data
                foreach (var d in gsrPpgFragments)
                {
                    var sheet = package.Workbook.Worksheets.Add(d.Key);

                    var query = from p in d.Value
                                select new { Timestamp = new TimeSpan(p.Timestamp).ToString("g"), p.Value };
                    sheet.Cells[1, 1].LoadFromCollection(query, PrintHeaders: true);
                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                    var table = sheet.Tables.Add(sheet.Dimension, d.Key);
                    // Set table properties (order matters!)
                    table.ShowRowStripes = false;
                    table.TableStyle = TableStyles.Medium14;
                    table.ShowFirstColumn = true;
                }

                package.Save();
            }

            return 0;
        }
    }
}
