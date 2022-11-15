using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Pancake.Monetization
{
    public class AdsUtil
    {
        /// <summary>
        /// Compares its two arguments for order.  Returns <see cref="EVersionComparisonResult.Lesser"/>, <see cref="EVersionComparisonResult.Equal"/>,
        /// or <see cref="EVersionComparisonResult.Greater"/> as the first version is less than, equal to, or greater than the second.
        /// </summary>
        /// <param name="versionA">The first version to be compared.</param>
        /// <param name="versionB">The second version to be compared.</param>
        /// <returns>
        /// <see cref="EVersionComparisonResult.Lesser"/> if versionA is less than versionB.
        /// <see cref="EVersionComparisonResult.Equal"/> if versionA and versionB are equal.
        /// <see cref="EVersionComparisonResult.Greater"/> if versionA is greater than versionB.
        /// </returns>
        public static EVersionComparisonResult CompareVersions(string versionA, string versionB)
        {
            if (versionA.Equals(versionB)) return EVersionComparisonResult.Equal;

            // Check if either of the versions are beta versions. Beta versions could be of format x.y.z-beta or x.y.z-betaX.
            // Split the version string into beta component and the underlying version.
            int piece;
            var isVersionABeta = versionA.Contains("-beta");
            var versionABetaNumber = 0;
            if (isVersionABeta)
            {
                var components = versionA.Split(new[] {"-beta"}, StringSplitOptions.None);
                versionA = components[0];
                versionABetaNumber = int.TryParse(components[1], out piece) ? piece : 0;
            }

            var isVersionBBeta = versionB.Contains("-beta");
            var versionBBetaNumber = 0;
            if (isVersionBBeta)
            {
                var components = versionB.Split(new[] {"-beta"}, StringSplitOptions.None);
                versionB = components[0];
                versionBBetaNumber = int.TryParse(components[1], out piece) ? piece : 0;
            }

            // Now that we have separated the beta component, check if the underlying versions are the same.
            if (versionA.Equals(versionB))
            {
                // The versions are the same, compare the beta components.
                if (isVersionABeta && isVersionBBeta)
                {
                    if (versionABetaNumber < versionBBetaNumber) return EVersionComparisonResult.Lesser;

                    if (versionABetaNumber > versionBBetaNumber) return EVersionComparisonResult.Greater;
                }
                // Only VersionA is beta, so A is older.
                else if (isVersionABeta)
                {
                    return EVersionComparisonResult.Lesser;
                }
                // Only VersionB is beta, A is newer.
                else
                {
                    return EVersionComparisonResult.Greater;
                }
            }

            // Compare the non beta component of the version string.
            var versionAComponents = versionA.Split('.').Select(version => int.TryParse(version, out piece) ? piece : 0).ToArray();
            var versionBComponents = versionB.Split('.').Select(version => int.TryParse(version, out piece) ? piece : 0).ToArray();
            var length = Mathf.Max(versionAComponents.Length, versionBComponents.Length);
            for (var i = 0; i < length; i++)
            {
                var aComponent = i < versionAComponents.Length ? versionAComponents[i] : 0;
                var bComponent = i < versionBComponents.Length ? versionBComponents[i] : 0;

                if (aComponent < bComponent) return EVersionComparisonResult.Lesser;

                if (aComponent > bComponent) return EVersionComparisonResult.Greater;
            }

            return EVersionComparisonResult.Equal;
        }

        public static bool IsInEEA()
        {
            string code = RegionInfo.CurrentRegion.Name;
            if (code.Equals("AT") || code.Equals("BE") || code.Equals("BG") || code.Equals("HR") || code.Equals("CY") || code.Equals("CZ") || code.Equals("DK") ||
                code.Equals("EE") || code.Equals("FI") || code.Equals("FR") || code.Equals("DE") || code.Equals("EL") || code.Equals("HU") || code.Equals("IE") ||
                code.Equals("IT") || code.Equals("LV") || code.Equals("LT") || code.Equals("LU") || code.Equals("MT") || code.Equals("NL") || code.Equals("PL") ||
                code.Equals("PT") || code.Equals("RO") || code.Equals("SK") || code.Equals("SI") || code.Equals("ES") || code.Equals("SE") || code.Equals("IS") ||
                code.Equals("LI") || code.Equals("NO"))
            {
                return true;
            }

            return false;
        }
    }
}