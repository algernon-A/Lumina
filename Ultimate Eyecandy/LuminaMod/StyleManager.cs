﻿namespace Lumina
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using static LuminaStyle;

    /// <summary>
    /// Handling of styles.
    /// </summary>
    internal static class StyleManager
    {
        private static float[] _activeSettings;
        private static List<LuminaStyle> _styleList = new List<LuminaStyle>();

        /// <summary>
        /// Gets the active settings arrray.
        /// </summary>
        internal static float[] ActiveSettings
        {
            get
            {
                if (_activeSettings == null)
                {
                    _activeSettings = new float[(int)ValueIndex.NumSettings];
                }

                return _activeSettings;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sky tonemapping is enabled.
        /// </summary>
        internal static bool EnableSkyTonemapping { get; set; }

        /// <summary>
        /// Gets the list of currently active styles.
        /// </summary>
        internal static List<LuminaStyle> StyleList => _styleList;

        /// <summary>
        /// Deletes the given style.
        /// </summary>
        /// <param name="style"></param>
        internal static void DeleteStyle(LuminaStyle style)
        {
            if (style != null && style.IsLocal && _styleList.Contains(style))
            {
                _styleList.Remove(style);

                // Delete directory if it exists.
                if (!style.FilePath.IsNullOrWhiteSpace() && File.Exists(style.FilePath))
                {
                    Directory.Delete(Path.GetDirectoryName(style.FilePath), true);
                }
            }
        }

        /// <summary>
        /// Applies current settings to the game.
        /// </summary>
        internal static void ApplySettings()
        {
            LuminaLogic.CalculateLighting(
                _activeSettings[(int)ValueIndex.Temperature],
                _activeSettings[(int)ValueIndex.Tint],
                _activeSettings[(int)ValueIndex.SunTemp],
                _activeSettings[(int)ValueIndex.SunTint],
                _activeSettings[(int)ValueIndex.SkyTemp],
                _activeSettings[(int)ValueIndex.SkyTint],
                _activeSettings[(int)ValueIndex.MoonTemp],
                _activeSettings[(int)ValueIndex.MoonTint],
                _activeSettings[(int)ValueIndex.MoonLight],
                _activeSettings[(int)ValueIndex.TwilightTint]);

            LuminaLogic.CalculateTonemapping(_activeSettings[(int)ValueIndex.Brightness], _activeSettings[(int)ValueIndex.Gamma], _activeSettings[(int)ValueIndex.Contrast]);
        }

        /// <summary>
        /// Finds and loads available styles.
        /// </summary>
        internal static void LoadStyles()
        {
            // Clear existing list.
            _styleList.Clear();

            // Determine filepath.
            string addonsPath = Path.Combine(DataLocation.localApplicationData, "Addons");
            string localModPath = Path.Combine(addonsPath, "Mods");

            // Ensure local mod directory exists.
            if (!Directory.Exists(localModPath))
            {
                Directory.CreateDirectory(localModPath);
            }

            // Dictionary for parsed values.
            Dictionary<int, float> fileData = new Dictionary<int, float>();

            // Iterate through each directory.
            foreach (string filename in Directory.GetFiles(localModPath, "*.light", SearchOption.AllDirectories))
            {
                Logging.Message("parsing lighting file ", filename);

                fileData.Clear();
                bool skyTonemapping = true;
                string styleName = null;

                try
                {
                    // Parse each line in file with an equals delimiter.
                    foreach (string line in File.ReadAllLines(filename).Where(s => s.Contains(" = ")))
                    {
                        string[] lineSplit = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);

                        if (lineSplit[0] == "skyTmpg")
                        {
                            skyTonemapping = bool.Parse(lineSplit[1]);
                        }
                        else if (lineSplit[0] == "name")
                        {
                            Logging.Message("reading style name ", lineSplit[1]);
                            styleName = lineSplit[1];
                        }
                        else
                        {
                            if (int.TryParse(lineSplit[0], out int index))
                            {
                                if (float.TryParse(lineSplit[1], out float value))
                                {
                                    fileData[index] = value;
                                }
                            }
                        }
                    }

                    // If no valid style name was parsed, use the filename.
                    if (!styleName.IsNullOrWhiteSpace())
                    {
                        styleName = Path.GetFileName(filename);
                    }

                    // Add style to list if it has a valid name.
                    if (!styleName.IsNullOrWhiteSpace())
                    {
                        Logging.Message("adding style ", styleName);
                        _styleList.Add(new LuminaStyle(
                            styleName,
                            fileData[(int)ValueIndex.Temperature],
                            fileData[(int)ValueIndex.Tint],
                            fileData[(int)ValueIndex.SunTemp],
                            fileData[(int)ValueIndex.SunTint],
                            fileData[(int)ValueIndex.SkyTemp],
                            fileData[(int)ValueIndex.SkyTint],
                            fileData[(int)ValueIndex.MoonTemp],
                            fileData[(int)ValueIndex.MoonTint],
                            fileData[(int)ValueIndex.MoonLight],
                            fileData[(int)ValueIndex.TwilightTint],
                            fileData[(int)ValueIndex.Brightness],
                            fileData[(int)ValueIndex.Gamma],
                            fileData[(int)ValueIndex.Contrast],
                            skyTonemapping,
                            true)
                        {
                            FilePath = Path.GetDirectoryName(filename),
                        });
                    }
                    else
                    {
                        Logging.Error("invalid style name for ", filename);
                    }
                }
                catch (Exception e)
                {
                    // Don't let a single failure stop us.
                    Logging.LogException(e, "exception parsing style file ", filename);
                }
            }
        }
    }
}
