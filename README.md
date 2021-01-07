Tadley : Muse / GSR / PPG Reader and Extractor
==============================================

by Jungwoo Nam. InsLab

## Build

**1. Clone this repo**

```powershell
New-Item -Name projects -ItemType directory
cd projects
git clone https://github.com/coloriz/Tadley.git
```

**2. Clone other dependencies**

```
cd ..
git clone https://github.com/coloriz/GsrPpgSampler.git
git clone https://github.com/coloriz/MuseLibrary.git
```

**3. Install NuGet packages**

**4. Open Tadley.sln and Build Solution**

## Usage

```powershell
> ./Tadley help
Tadley 1.0.0.0
Copyright © 2019. InS Lab. Dept. of C.E. SKUniv. All rights reserved.

  record     Record data coming from muse.

  extract    Extract data from recorded file.

  help       Display more information on a specific command.

  version    Display version information.
```

```powershell
> ./Tadley record --help
Tadley 1.0.0.0
Copyright © 2019. InS Lab. Dept. of C.E. SKUniv. All rights reserved.

  -d, --data      (Default: EEG) Data to be read from Muse (Default : EEG)

  -p, --port      Required. GSR/PPG Sampler COM port

  -o, --output    Required. Output file to be written

  --help          Display this help screen.

  --version       Display version information.
```

```powershell
> ./Tadley extract --help
Tadley 1.0.0.0
Copyright © 2019. InS Lab. Dept. of C.E. SKUniv. All rights reserved.

  -i, --input       Required. Input file to be processed

  -s, --timeoff     Required. Set the start time offset

  -t, --duration    Required. Set the duration (end time = start time + duration)

  -o, --output      Required. Output file to be written

  --help            Display this help screen.

  --version         Display version information.
```

## Example

**Record**
```powershell
./Tadley record -d EEG:DeltaAbsolute:ThetaAbsolute:AlphaAbsolute:BetaAbsolute:GammaAbsolute:DeltaRelative:ThetaRelative:AlphaRelative:BetaRelative:GammaRelative -p COM3 -o 190527.json
```

**Extract**
```powershell
./Tadley extract -i 190527.json -s 0:00:00 -t 0:00:30 -o 190527.xlsx
```

### Sample data

- [190527.json](samples/190527.json)
- [190527.xlsx](samples/190527.xlsx)