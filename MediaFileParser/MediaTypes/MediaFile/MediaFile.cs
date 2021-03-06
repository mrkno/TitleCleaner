﻿#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MediaFileParser.Common;

#endregion

namespace MediaFileParser.MediaTypes.MediaFile
{
    /// <summary>
    /// Superclass of all media files
    /// </summary>
    public abstract partial class MediaFile
    {
        /// <summary>
        /// Delimeter chars of separator strings.
        /// </summary>
        private static readonly char[] DelimChars = {',', '.', '_', ' '};

        /// <summary>
        /// OS Path Separators
        /// </summary>
        protected static readonly char[] PathSeperators = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        /// <summary>
        /// List of sectors of the media file name.
        /// </summary>
        protected List<string> SectorList;

        /// <summary>
        /// Storage of the default ToString() output format.
        /// </summary>
        protected static string DefaultFormatToString = "C?( (P)).E";

        /// <summary>
        /// Gets or sets the default ToString() output format.
        /// </summary>
        public static string DefaultFormatString
        {
            get { return DefaultFormatToString; }
            set { DefaultFormatToString = value; }
        }

        /// <summary>
        /// Storage of the output directory for this media type.
        /// </summary>
        protected static string TypeOutDirectory = "Media";

        /// <summary>
        /// Gets or sets the default directory name that this
        /// media file type should be stored in if it moves directory.
        /// </summary>
        public static string TypeDirectory
        {
            get { return TypeOutDirectory; }
            set { TypeOutDirectory = value; }
        }

        /// <summary>
        /// Gets the directory name that this media file type
        /// should be stored in if it moves directory.
        /// See static member TypeDirectory for manipulation.
        /// </summary>
        public virtual string OutputDirectory
        {
            get { return TypeDirectory; }
        }

        /// <summary>
        /// Constructor for a media file.
        /// Performs common tasks for name cleanup.
        /// </summary>
        /// <param name="file">Name to clean.</param>
        protected MediaFile(string file)
        {
            Origional = file;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Year = -1;  // no matter how we override, it should always be init to -1

            // Location
            int pathSep;
            if ((pathSep = file.LastIndexOfAny(PathSeperators)) != -1)
            {
                Location = file.Substring(0, pathSep);
                Origional = Origional.Substring(pathSep + 1);
            }

            // Extension
            var pathExt = file.LastIndexOf('.');
            Extension = file.Substring(pathExt + 1);
            Origional = Origional.Substring(0, Origional.Length - Extension.Length - 1);

            // Raw
            file = Origional;
            SectorList = new List<string>(file.Split(DelimChars, StringSplitOptions.RemoveEmptyEntries));
            
            int year = -1, removeStart = int.MaxValue;
            var lastWasSingle = false;
            for (var i = 0; i < SectorList.Count; i++)
            {
                // And add splits along num-letter boundries
                var match = Regex.Match(SectorList[i], @"([0-9])+-([a-zA-Z])+");
                if (match.Success)
                {
                    var ind = match.Value.IndexOf('-') + match.Index;
                    if (!SectorList[i].Substring(ind+1).Contains("-"))     // Cope with strings such as 100-year-old
                    {
                        SectorList.Insert(i + 1, SectorList[i].Substring(ind + 1));
                        SectorList[i] = SectorList[i].Substring(0, ind);
                    }
                }
                if (String.IsNullOrEmpty(SectorList[i])) continue;

                // Autocapitalise first letter in each word
                SectorList[i] = ToUpperString(SectorList[i]);

                // Merge alone letters. eg "A M" -> "AM"
                if (Regex.IsMatch(SectorList[i], @"^[A-Z]$"))
                {
                    if (lastWasSingle)
                    {
                        SectorList[i - 1] += SectorList[i];
                        SectorList.RemoveAt(i);
                        i--;
                    }
                    lastWasSingle = true;
                }
                else
                {
                    lastWasSingle = false;
                }

                // Detect part/disk numbers
                if (Regex.IsMatch(SectorList[i], "^(part|cd|disk)([0-9]+)?$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                {                                // note: no dvd as this conflicts with junk names
                    var regex = Regex.Match(SectorList[i], "[0-9]+$", RegexOptions.IgnorePatternWhitespace);
                    if (!regex.Success && i + 1 < SectorList.Count)
                    {
                        FromNumberWord(ref SectorList, i + 1, true);
                        regex = Regex.Match(SectorList[i + 1], "^[0-9]+$", RegexOptions.IgnorePatternWhitespace);
                        SectorList.RemoveAt(i + 1);
                    }

                    if (regex.Success)
                    {
                        Part = uint.Parse(regex.Value);
                        SectorList.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                // Detect junk strings
                DetectJunkAtIndex(ref i, ref year, ref removeStart);
            }

            if (removeStart == int.MaxValue)
            {
                removeStart = SectorList.Count;
            }

            // Remove junk names
            SectorRangeRemove(removeStart, year);
        }

        /// <summary>
        /// Origional file name for this file (excluding file extension).
        /// </summary>
        public string Origional { get; protected set; }

        /// <summary>
        /// File extension for this media file.
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        /// Location on the HD that this media file is stored.
        /// </summary>
        public string Location { get; protected set; }

        /// <summary>
        /// Year that the media in this media file was made.
        /// </summary>
        public virtual int Year { get; protected set; }

        /// <summary>
        /// Part of the file if it is split into segments.
        /// 0 if is not segmented or segment number is unknown.
        /// </summary>
        public uint Part { get; protected set; }

        /// <summary>
        /// Quality of the media file.
        /// </summary>
        public MediaFileQuality? Quality { get; protected set; }
        
        /// <summary>
        /// Folder this file is stored in.
        /// </summary>
        public string Folder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Location) || Location.LastIndexOfAny(PathSeperators) == -1)
                {
                    return "";      // not stored in a directory (probably root)
                }
                return Location.Substring(Location.LastIndexOfAny(PathSeperators) + 1);
            }
        }

        /// <summary>
        /// Gets the default cleaned name of this file.
        /// </summary>
        public abstract string Cleaned { get; }

        /// <summary>
        /// Tests if the media file is of this type.
        /// </summary>
        /// <returns>If this file is of this media type.</returns>
        public abstract bool Test();

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return ToString(DefaultFormatString);
        }

        /// <summary>
        /// Returns the string representation of this object based on character in format string.
        /// </summary>
        /// <param name="str">
        /// Format string to base this representation on.
        /// L:  Location
        /// O:  Origional Filename
        /// C:  Cleaned Filename
        /// E:  File Extension
        /// Y:  Year (or if year is unknown, current year)
        /// P:  Part of File
        /// ?:  Only return next character group if it is not null/empty/whitespace
        /// \:  Return Next Character
        /// </param>
        /// <param name="ind">The index of the character to base the string representation on.</param>
        /// <returns>The string representation of this object based on character at the specified index.</returns>
        public virtual string ToString(ref string str, ref int ind)
        {
            switch (str[ind])
            {
                case 'L':
                {
                    return Location;
                }
                case 'O':
                {
                    return Origional;
                }
                case 'C':
                {
                    return Cleaned;
                }
                case 'E':
                {
                    return Extension;
                }
                case 'Y':
                {
                    return Year <= 0 ? "" : Year.ToString(CultureInfo.InvariantCulture);
                }
                case 'P':
                {
                    return Part == 0 ? "" : Part.ToString(CultureInfo.InvariantCulture);
                }
                case '?':
                {
                    var inc = 0;
                    var sub = "";
                    if (ind + 1 != str.Length)
                    {
                        if (str[ind + 1] == '(')
                        {
                            sub = str.GetBracketedString(ind + 1, ref _bracketedStringLookupDictionary);
                            inc = sub.Length + 2;
                            sub = string.IsNullOrWhiteSpace(sub) ? "" : ToString(sub);
                            if (string.IsNullOrWhiteSpace(sub) || sub == str.Substring(ind+1, inc-2) || sub.Length < inc-2)
                            {
                                sub = "";   // bracket only contains escaped characters
                            }
                        }
                        else
                        {
                            inc = 1;
                            var ind1 = ind + 1;
                            sub = ToString(ref str, ref ind1);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(sub))
                    {
                        sub = "";
                    }

                    ind += inc;
                    return sub;
                }
                case '\\':
                {
                    ind++;
                    goto default;
                }
                default:
                {
                    return ind >= str.Length ? "" : str[ind].ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <param name="str">
        /// Format string to base this representation on.</param>
        /// <returns>The string representation of this object.</returns>
        public string ToString(string str)
        {
            var result = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                result.Append(ToString(ref str, ref i));
            }
            return result.ToString();
        }

        /// <summary>
        /// Dictionary to store bracketed strings so that they can be looked up quickly in repetitive cases.
        /// </summary>
        private static Dictionary<string, string> _bracketedStringLookupDictionary = new Dictionary<string, string>();

        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Words for numbers
        /// </summary>
        private enum NumberWord
        {
            Zero = 0, One = 1, Two = 2, Three = 3, Four = 4, Five = 5,
            Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
        }   

        /// <summary>
        /// Roman numerals for numbers
        /// </summary>
        private enum NumberRoman
        {
            I = 1, II = 2, III = 3, IV = 4, V = 5,
            VI = 6, VII = 7, VIII = 8, XI = 9, X = 10
        }
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local InconsistentNaming

        /// <summary>
        /// Converts a word to a number and replaces it in an array with the numerical
        /// representation.
        /// </summary>
        /// <param name="arr">Array to use</param>
        /// <param name="index">Index of the array to read and replace.</param>
        /// <param name="roman">Also convert roman numerals.</param>
        /// <returns>Success.</returns>
        protected static bool FromNumberWord(ref List<string> arr, int index, bool roman = false)
        {
            NumberWord word;
            if (!Enum.TryParse(arr[index], true, out word))
            {
                if (roman)
                {
                    NumberRoman numeral;
                    if (!Enum.TryParse(arr[index], true, out numeral)) return false;
                    word = (NumberWord)(int) numeral;
                }
                else
                {
                    return false;
                }
            }
            var num = ((int)word).ToString(CultureInfo.InvariantCulture);
            arr[index] = num;
            return true;
        }

        /// <summary>
        /// Converts a string to uppercase.
        /// </summary>
        /// <param name="str">String to make uppercase.</param>
        /// <returns>Uppercase string.</returns>
        protected string ToUpperString(string str)
        {
            var a = str.ToCharArray();
            a[0] = Char.ToUpper(a[0], CultureInfo.InvariantCulture);
            return new string(a);
        }
    }
}