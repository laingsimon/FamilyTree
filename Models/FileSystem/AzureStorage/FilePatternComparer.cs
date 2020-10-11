using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
    internal class FilePatternComparer : IEqualityComparer<string>
    {
        public bool Equals(string fileName, string filePattern)
        {
            if (string.IsNullOrEmpty(filePattern) || filePattern == "*" || filePattern == "*.*")
            {
                return true;
            }

            var regex = string.Join("", BuildRegexPattern(filePattern));
            return Regex.IsMatch(fileName, regex);
        }

        private IEnumerable<string> BuildRegexPattern(string filePattern)
        {
            var patternParts = filePattern.Split('*');
            
            if (!filePattern.StartsWith("*"))
            {
                yield return "^";
            }

            for (var index = 0; index < patternParts.Length; index++)
            {
                var part = patternParts[index];
                if (index > 0)
                {
                    yield return ".*";
                }

                yield return Regex.Escape(part);
            }

            if (!filePattern.EndsWith("*"))
            {
                yield return "$";
            }
        }

        public int GetHashCode(string fileName)
        {
            return 0; //need to find a better solution to this.
        }
    }
}