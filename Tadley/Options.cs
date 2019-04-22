using System;
using System.Collections.Generic;
using CommandLine;
using InsLab.Muse;

namespace Tadley
{
    [Verb("record", HelpText = "Record data coming from muse.")]
    class RecordOptions
    {
        // Muse Options
        [Option('d', "data", Default = new MuseData[] { MuseData.EEG }, HelpText = "Data to be read from Muse (Default : EEG)", Separator = ':')]
        public IEnumerable<MuseData> Data { get; set; }

        // GsrPpgSampler Options
        [Option('p', "port", Required = true, HelpText = "GSR/PPG Sampler COM port")]
        public string PortName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file to be written")]
        public string OutputFile { get; set; }
    }

    [Verb("extract", HelpText = "Extract data from recorded file.")]
    class ExtractOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file to be processed")]
        public string InputFile { get; set; }

        [Option('s', "timeoff", Required = true, HelpText = "Set the start time offset")]
        public TimeSpan TimeOff { get; set; }

        [Option('t', "duration", Required = true, HelpText = "Set the duration (end time = start time + duration)")]
        public TimeSpan Duration { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file to be written")]
        public string OutputFile { get; set; }
    }
}
