using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PancakeEditor.Finder
{
    internal static class FinderShaderGraphReader
    {
        // Define a regex pattern to search for the target string, with an optional type field
        private static readonly Regex Pattern = new("{\\\\\"fileID\\\\\":(\\d+),\\\\\"guid\\\\\":\\\\\"([a-f0-9]{32})\\\\\"");

        // The function to read and process .shadergraph files
        public static List<(long fileID, string guid)> ExtractFileIDGuidPairs(string shaderGraphPath)
        {
            // Read the entire content of the shadergraph file
            string fileContent = File.ReadAllText(shaderGraphPath);

            // Find all matches of the pattern
            var matches = Pattern.Matches(fileContent);

            // Use a HashSet to store unique pairs
            HashSet<(long fileID, string guid)> uniquePairs = new HashSet<(long fileID, string guid)>();

            foreach (Match match in matches)
            {
                // Extract fileID and guid from the match
                long fileID = long.Parse(match.Groups[1].Value);
                string guid = match.Groups[2].Value;

                // Add the extracted pair to the set
                uniquePairs.Add((fileID, guid));
            }

            // Convert the set to a list and return
            return new List<(long fileID, string guid)>(uniquePairs);
        }
    }
}