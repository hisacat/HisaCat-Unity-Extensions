using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HisaCat
{
    public static class CSVUtil
    {
        // Constants for CSV boolean representation.
        private const string CSVBooleanTrueString = "1";
        private const string CSVBooleanFalseString = "0";

        // Regex patterns for splitting CSV fields and lines (declared as constants).
        private const string FieldSplitPattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private const string LineSplitPattern = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TrimChars = { '\"' };

        /// <summary>
        /// Reads a CSV string and returns a list of dictionaries representing each row.
        /// </summary>
        public static List<Dictionary<string, object>> Read(string csvContent)
        {
            var records = new List<Dictionary<string, object>>();

            // Split the CSV content into lines.
            string[] lines = Regex.Split(csvContent, LineSplitPattern);
            if (lines.Length <= 1)
                return records;

            // Use the first line as headers.
            string[] headers = Regex.Split(lines[0], FieldSplitPattern);

            // Process each line starting from the second line as data rows.
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] fields = Regex.Split(lines[i], FieldSplitPattern);
                if (fields.Length == 0 || string.IsNullOrEmpty(fields[0]))
                    continue;

                Dictionary<string, object> record = new();
                int fieldCount = Math.Min(headers.Length, fields.Length);
                for (int j = 0; j < fieldCount; j++)
                {
                    string field = fields[j].TrimStart(TrimChars)
                                            .TrimEnd(TrimChars)
                                            .Replace("\\", "");

                    object finalValue = field;
                    if (int.TryParse(field, out int intValue))
                        finalValue = intValue;
                    else if (float.TryParse(field, out float floatValue))
                        finalValue = floatValue;

                    record[headers[j]] = finalValue;
                }
                records.Add(record);
            }
            return records;
        }

        /// <summary>
        /// Reads a CSV string and converts each row into a struct of type TData.<br/>
        /// TData must be a struct with public fields that match the CSV header names.<br/>
        /// Special handling for bool: CsvBooleanFalseString returns false, CsvBooleanTrueString returns true; any other value throws an exception.<br/>
        /// </summary>
        public static List<TData> Read<TData>(string csvContent, bool warnIfFieldMissing = true) where TData : struct
        {
            var results = new List<TData>();

            // Split the CSV content into lines.
            string[] lines = Regex.Split(csvContent, LineSplitPattern);
            if (lines.Length <= 1)
                return results;

            // Get headers from the first line.
            string[] headers = Regex.Split(lines[0], FieldSplitPattern);

            // Build a dictionary mapping header to its index for quick lookup (case-insensitive).
            var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerIndex[headers[i]] = i;
            }

            // Get public instance fields of TData.
            FieldInfo[] fieldInfos = typeof(TData).GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (warnIfFieldMissing)
            {
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    var field = fieldInfos[i];
                    if (headerIndex.ContainsKey(field.Name) == false)
                        Debug.LogWarning($"[{nameof(CSVUtil)}] '{typeof(TData).Name}' has a field named '{field.Name}' that is not present in the CSV headers.");
                }
            }

            // Process each data row.
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] columns = Regex.Split(lines[i], FieldSplitPattern);
                if (columns.Length == 0 || string.IsNullOrEmpty(columns[0]))
                    throw new FormatException("Encountered an empty row or missing required data in the first column.");

                TData data = new TData();
                for (int j = 0; j < fieldInfos.Length; j++)
                {
                    FieldInfo field = fieldInfos[j];
                    if (headerIndex.TryGetValue(field.Name, out int index))
                    {
                        if (index < columns.Length)
                        {
                            string rawValue = columns[index]
                                .TrimStart(TrimChars)
                                .TrimEnd(TrimChars)
                                .Replace("\\", "");
                            object convertedValue = ConvertToType(rawValue, field.FieldType);
                            field.SetValueDirect(__makeref(data), convertedValue);
                        }
                    }
                }
                results.Add(data);
            }

            return results;
        }

        /// <summary>
        /// Converts a string value to the specified target type.<br/>
        /// Special handling for bool: CsvBooleanFalseString returns false, CsvBooleanTrueString returns true.<br/>
        /// Throws FormatException if conversion fails.<br/>
        /// </summary>
        private static object ConvertToType(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(bool))
            {
                if (value == CSVBooleanFalseString)
                    return false;
                else if (value == CSVBooleanTrueString)
                    return true;
                else
                    throw new FormatException($"Invalid boolean value: '{value}'. Expected '{CSVBooleanFalseString}' or '{CSVBooleanTrueString}'.");
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(value, out int intValue))
                    return intValue;
                else
                    throw new FormatException($"Invalid int value: '{value}'.");
            }

            if (targetType == typeof(float))
            {
                if (float.TryParse(value, out float floatValue))
                    return floatValue;
                else
                    throw new FormatException($"Invalid float value: '{value}'.");
            }

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Could not convert '{value}' to type {targetType.Name}.", ex);
            }
        }

        /// <summary>
        /// Converts a list of dictionaries (CSV data) to a CSV formatted string.
        /// </summary>
        public static string Write(List<Dictionary<string, object>> csvData)
        {
            if (csvData == null || csvData.Count == 0)
                return string.Empty;

            // Determine header order: use keys from the first record and add any additional keys from subsequent records.
            List<string> headerList = new();

            // Add keys from the first record.
            string[] keys0 = new string[csvData[0].Keys.Count];
            csvData[0].Keys.CopyTo(keys0, 0);
            for (int i = 0; i < keys0.Length; i++)
                headerList.Add(keys0[i]);

            // Add keys from all records.
            for (int i = 0; i < csvData.Count; i++)
            {
                Dictionary<string, object> record = csvData[i];
                string[] keys = new string[record.Keys.Count];
                record.Keys.CopyTo(keys, 0);
                for (int j = 0; j < keys.Length; j++)
                {
                    if (headerList.Contains(keys[j]) == false)
                        headerList.Add(keys[j]);
                }
            }

            StringBuilder sb = new();

            // Write the header row with escaped fields.
            string[] escapedHeaders = new string[headerList.Count];
            for (int i = 0; i < headerList.Count; i++)
                escapedHeaders[i] = EscapeCsvField(headerList[i]);
            sb.AppendLine(string.Join(",", escapedHeaders));

            // Write each data row.
            for (int i = 0; i < csvData.Count; i++)
            {
                Dictionary<string, object> record = csvData[i];
                string[] row = new string[headerList.Count];
                for (int j = 0; j < headerList.Count; j++)
                {
                    record.TryGetValue(headerList[j], out object value);
                    row[j] = EscapeCsvField(value);
                }
                sb.AppendLine(string.Join(",", row));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a list of TData (CSV data) to a CSV formatted string.<br/>
        /// TData must be a struct with public fields.<br/>
        /// </summary>
        public static string Write<TData>(List<TData> csvData)
        {
            if (csvData == null || csvData.Count == 0)
                return string.Empty;

            Type type = typeof(TData);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Sort fields by metadata token to preserve the declaration order.
            Array.Sort(fields, (f1, f2) => f1.MetadataToken.CompareTo(f2.MetadataToken));

            StringBuilder sb = new();

            // Write the header row with field names.
            string[] headerList = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
                headerList[i] = EscapeCsvField(fields[i].Name);
            sb.AppendLine(string.Join(",", headerList));

            // Write each data row by retrieving the value of each field.
            for (int i = 0; i < csvData.Count; i++)
            {
                TData item = csvData[i];
                string[] rowList = new string[fields.Length];
                for (int j = 0; j < fields.Length; j++)
                {
                    object value = fields[j].GetValue(item);
                    rowList[j] = EscapeCsvField(value);
                }
                sb.AppendLine(string.Join(",", rowList));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escapes a CSV field by handling commas, quotes, and newlines appropriately.<br/>
        /// If the value is a bool, it converts it to CsvBooleanTrueString for true and CsvBooleanFalseString for false.<br/>
        /// </summary>
        private static string EscapeCsvField(object value)
        {
            if (value == null)
                return string.Empty;

            // Convert bool values to CSV-specific string representations.
            if (value is bool boolValue)
                value = boolValue ? CSVBooleanTrueString : CSVBooleanFalseString;

            string field = value.ToString();
            if (field.IndexOf('\"') != -1)
                field = field.Replace("\"", "\"\"");
            if (field.IndexOfAny(new[] { ',', '\r', '\n', '\"' }) != -1)
                field = $"\"{field}\"";
            return field;
        }
    }
}
