using System.Collections.Generic;

namespace WUWA_Setting {
    internal class SettingDataList : List<SettingStructure> {
        internal SettingDataList () {
            this.AddRange(
                new List<SettingStructure>() {
                    {
                        new SettingStructure () {
                            Name = "CustomFrameRate",
                            Text = "Frame Rate",
                            Value = 3,
                            DataTypeOption = new List<string> { "30", "45", "60", "120" },
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "PcVsync",
                            Text = "V-Sync",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "AntiAliasing",
                            Text = "Anti Aliasing",
                            Value = 0,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "RayTracing",
                            Text = "Ray Tracing",
                            Value = 0,
                            DataTypeOption = new List < string > { "Off", "Low", "Medium", "High" },
                            IsNewColumn = true,
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "RayTracedGI",
                            Text = "RT Global Illumination",
                            Value = 0,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "RayTracedReflection",
                            Text = "RT Reflections",
                            Value = 0,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "RayTracedShadow",
                            Text = "RT Shadows",
                            Value = 0,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "NvidiaSuperSamplingEnable",
                            Text = "NVIDIA DLSS",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                            IsNewColumn = true,
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "NvidiaSuperSamplingFrameGenerate",
                            Text = "DLSS Frame Generation",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "NvidiaSuperSamplingQuality",
                            Text = "DLSS Resolution",
                            Value = 99,
                            DataTypeOption = new List < string > { "Auto", "Ult. Perf", "Perf", "Balanced", "Quality", "Ult. Quality" },
                        }
                    },
                    // auto = 99, balanced = 0, rest is negative/positive
                    {
                        new SettingStructure () {
                            Name = "NvidiaSuperSamplingSharpness",
                            Text = "DLSS Sharpening",
                            Value = 0,
                            DataTypeOption = typeof(float),
                            Button = new SettingButton () {
                                Enabled = false,
                            },
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "FsrEnable",
                            Text = "AMD FSR",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                            IsNewColumn = true,
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "XessEnable",
                            Text = "Intel XeSS",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "XessQuality",
                            Text = "XeSS Quality",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                            Button = new SettingButton () {
                                Enabled = false,
                            },
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "IrxEnable",
                            Text = "Intel Iris Xe",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                        }
                    },
                    {
                        new SettingStructure () {
                            Name = "MetalFxEnable",
                            Text = "Apple MetalFX",
                            Value = 1,
                            DataTypeOption = typeof(bool),
                        }
                    },
                }
            );
        }
    }
}
