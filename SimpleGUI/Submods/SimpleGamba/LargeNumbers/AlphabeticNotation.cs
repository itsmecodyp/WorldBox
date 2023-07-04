// ----------------------------------------------------------------------------
// LargeNumbers.AlphabeticNotation
// (c) Milan Egon Votrubec
//
// For documentation, please see the included "Large Numbers Library Welcome.pdf" file.
//
// For example usage, please see the "largenumbers.unity" example scene.
//
// Please see the "license.txt" file for licensing.
// ----------------------------------------------------------------------------

// If you're sure your code won't deal with NaN's or Inifinity's, you can uncomment this for a potential micro-speed bump.
// #define SKIPCHECKS

using System;
using System.Globalization;
using System.Text;

namespace SimpleGUI.Submods.SimpleGamba.LargeNumbers {
    /// <summary>
    /// An Alphabetic Notation based number.
    /// This is a similar number sysem used by other popular idle clicker games, with a slight difference. The
    /// letters used don't start with "aa". Instead, they start with "a" and then can grow to "z...z".
    /// </summary>
#if UNITY_2018_4_OR_NEWER
    [Serializable]
#endif
    public struct AlphabeticNotation : IEquatable<AlphabeticNotation> {
        const char NULL_CHARACTER = (char)0;

        // This list helps reduce Maths.Pow calls, but also idicates the precision of mathematic operations. Values that are deemed too far different in scale are ignored.
        // The largest long value is 9,223,372,036,854,775,807
        private static readonly double[] _powers = {
            1,
            1_000d,
            1_000_000d,
            1_000_000_000d,
            1_000_000_000_000d,
            1_000_000_000_000_000d,
            1_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000_000_000_000_000d,
            1_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000d
        };

        static internal readonly string[] _standardNames =
        {
            "",
            "K",
            "M",
            "B",
            "T"
        };

        static internal readonly char[] Alphabet = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        // int32 max 2_147_483_647, min -2_147_483_648
        static internal readonly int[] _base26tobase10 = {
            1,
            26,
            676,
            17_576,
            456_976,
            11_881_376,
            308_915_776
        };

        static internal readonly long[] _base26tobase10limits = {
            0,
            26,
            702,
            18_278,
            475_254,
            12_356_630,
            321_272_406
        };

        private static readonly StringBuilder _sb = new StringBuilder(16);
        private static readonly StringBuilder _alphabeticName = new StringBuilder(12);         // In a standard single-thread Unity game, this should pose no problem.
        private static readonly char[] _alphabeticChars = new char[_base26tobase10.Length];   // In a standard single-thread Unity game, this should pose no problem.

        /// <summary>
        /// The coefficient is the value, or the number, that preceeds the exponent part of the alphabetic notation.
        /// </summary>
        public double coefficient;

        /// <summary>
        /// Magnitude refers to the exponent of the alphabetic notation.
        /// </summary>
        public int magnitude;

        public bool isZero => coefficient == 0;
        public static AlphabeticNotation zero => new AlphabeticNotation();


        // ---------------------------------------------------------------------------- Constructors
        /// <summary>
        /// Create a AlphabeticNotation number from a real double value.
        /// </summary>
        /// <param name="real">A double type value to be converted into alphabetic notation.</param>
        public AlphabeticNotation(double value)
        {
#if SKIPCHECKS
            if ( value == 0 )
#else
            if(value == 0 || double.IsInfinity(value) || double.IsNaN(value))
#endif
            {
                coefficient = 0;
                magnitude = 0;
                return;
            }

            var m = 0;
            Fix(ref value, ref m);

            coefficient = value;
            magnitude = m;
        }


        /// <summary>
        /// Create a AlphabeticNotation number from a coefficient and a magnitude.
        /// </summary>
        /// <param name="c">A double type representing the coefficient.</param>
        /// <param name="m">An integer representing the exponent (magnitude) of the alphabetic notation.</param>
        public AlphabeticNotation(double c, int m)
        {
#if SKIPCHECKS
            if ( c == 0 )
#else
            if(c == 0 || double.IsInfinity(c) || double.IsNaN(c))
#endif
            {
                coefficient = magnitude = 0;
                return;
            }

            Fix(ref c, ref m);
            coefficient = c;
            magnitude = m;
        }


        // ---------------------------------------------------------------------------- Methods
        /// <summary>
        /// This method allows the developer to get just the alphabetic magnitude name from a given magnitude.
        /// </summary>
        /// <param name="magnitude">The magnitude of the alphabetic number.</param>
        public static string GetAlphabeticMagnitudeName(int magnitude)
        {
            if(magnitude == 0)
                return string.Empty;
            var absMagnitude = magnitude < 0 ? -magnitude : magnitude;

            // Check the min and max magnitudes.
            if(absMagnitude > int.MaxValue) {
                if(magnitude > 0)
                    return "∞";
                return "-∞";
            }

            _alphabeticName.Clear();

            if(absMagnitude == 0)
                return string.Empty;
            if(absMagnitude < _standardNames.Length)                  // Check for the standard KMBT names.
            {
                if(magnitude > 0)
                    return _standardNames[magnitude];

                if(magnitude < 0)
                    return _standardNames[absMagnitude] + "ᵗʰ";
            }

            var adjustedMagnitude = absMagnitude - _standardNames.Length;
            if(adjustedMagnitude < Alphabet.Length)                  // Let's deal with single character values.
            {
                _alphabeticName.Append(Alphabet[adjustedMagnitude]);
                if(magnitude < 0)
                    _alphabeticName.Append("ᵗʰ");
                return _alphabeticName.ToString();
            }

            var found = false;
            var valueIndex = 0;
            //TODO: for performance, is clearing a static array quicker than creating a new local array here? Certainly there would be less garbage created. 
            for(int i = 0, count = _base26tobase10.Length; i < count; ++i)
                _alphabeticChars[i] = NULL_CHARACTER;
            for(var i = _base26tobase10.Length - 1; i >= 0; --i, ++valueIndex) {
                if(adjustedMagnitude < _base26tobase10limits[i]) {
                    if(found)                                        // If there are already found characters, we can't have spaces in the sequence.
                    {
                        _alphabeticChars[valueIndex] = Alphabet[Alphabet.Length - 1];
                        _alphabeticChars[valueIndex - 1] -= (char)1;
                    }
                    continue;
                }

                int _m = adjustedMagnitude / _base26tobase10[i];
                if(_m > 0)
                    found = true;
                if(_m > 26)
                    _m = 26;
                adjustedMagnitude -= _m * _base26tobase10[i];
                if(found)
                    _alphabeticChars[valueIndex] = Alphabet[_m - (i > 0 ? 1 : 0)];
            }

            for(var i = 0; i < valueIndex; i++)
                if(_alphabeticChars[i] != NULL_CHARACTER)
                    _alphabeticName.Append(_alphabeticChars[i]);

            if(magnitude < 0)
                _alphabeticName.Append("ᵗʰ");
            return _alphabeticName.ToString();
        }


        /// <summary>
        /// Gets an AlphabeticNotation item from the given string if possible.
        /// </summary>
        /// <param name="alphabeticNotationString">The string to parse.</param>
        /// <param name="alphabeticNotation">The AlphabeticNotation item if successful.</param>
        /// <returns>true if the operation succeeds.</returns>
        /// <remarks>With the string garbage created, this method is best used infrequently, or during development and testing.</remarks>
        public static bool GetAlphabeticNotationFromString(string alphabeticNotationString, out AlphabeticNotation alphabeticNotation)
        {
            if(string.IsNullOrEmpty(alphabeticNotationString)) { alphabeticNotation = default; return false; }
            alphabeticNotationString = alphabeticNotationString.Trim();
            var count = alphabeticNotationString.Length;

            int spaces = 0;// alphabeticNotationString.Count ( x => x == ' ' ); // Removed the Linq requirement.
            for(int i = 0; i < count; ++i)
                if(alphabeticNotationString[i] == ' ')
                    ++spaces;

            string[] componenets;

            if(spaces > 1) {
                alphabeticNotation = default;
                return false;
            }

            if(spaces == 0) {
                var i = 0;
                for(; i < count; ++i) {
                    if((alphabeticNotationString[i] >= '0' && alphabeticNotationString[i] <= '9') || alphabeticNotationString[i] == '.' || alphabeticNotationString[i] == ',' || alphabeticNotationString[i] == '-')
                        continue;
                    if(i == 0) { alphabeticNotation = default; return false; }
                    break;
                }
                componenets = new string[2];
                componenets[0] = alphabeticNotationString.Substring(0, i);
                componenets[1] = i < count ? alphabeticNotationString.Substring(i) : string.Empty;
            }
            else // spaces == 1
            {
                componenets = alphabeticNotationString.Split(' ');
                if(componenets.Length != 2) { alphabeticNotation = default; return false; }   // Safety check.
            }

            float c;
            int m = 0;

            var provider = CultureInfo.CreateSpecificCulture("en-US");
            try {
                c = float.Parse(componenets[0], provider);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch { alphabeticNotation = default; return false; }
#pragma warning restore CA1031 // Do not catch general exception types

            if(!string.IsNullOrEmpty(componenets[1]))
                if(!GetMagnitudeFromAlphabeticName(componenets[1], out m)) {
                    alphabeticNotation = default;
                    return false;
                }

            alphabeticNotation = new AlphabeticNotation(c, m);
            return true;
        }


        /// <summary>
        /// This method will accept an alphabetic name as a string (with an optional ᵗʰ to indicate negative magnitudes, not to be mistaken with negative values), and returns a signed integer.
        /// </summary>
        /// <param name="alphabeticName">A valid AlphabeticNotation name as a string.</param>
        /// <param name="magnitude">The returned magnitude if the method succeeds.</param>
        /// <returns>A boolena indicating if the methdod succeeds.</returns>
        /// <remarks>With the string garbage created, this method is best used infrequently, or during development and testing.</remarks>
        public static bool GetMagnitudeFromAlphabeticName(string alphabeticName, out int magnitude)
        {
            if(string.IsNullOrEmpty(alphabeticName)) { magnitude = 0; return false; }
            var fraction = false;
            if(alphabeticName.EndsWith("ᵗʰ")) {
                fraction = true;
                alphabeticName = alphabeticName.Substring(0, alphabeticName.Length - 2);
            }
            if(alphabeticName.Length > 7) { magnitude = 0; return false; }
            if(alphabeticName.Length == 1) {
                for(int i = 0, count = _standardNames.Length; i < count; ++i)
                    if(alphabeticName == _standardNames[i]) { magnitude = fraction ? -i : i; return true; }
            }

            {
                magnitude = 0;
                var alphabeticNameIndex = alphabeticName.Length - 1;
                var alphabetCount = Alphabet.Length;
                for(int i = alphabeticNameIndex, base26tobase10Index = 0; i >= 0; --i, ++base26tobase10Index) {
                    if(alphabeticName[i] < 'a' || alphabeticName[i] > Alphabet[alphabetCount - 1]) { magnitude = 0; return false; }
                    for(int j = 0; j < alphabetCount; ++j) {
                        if(alphabeticName[i] == Alphabet[j]) {
                            if(alphabeticNameIndex > 0 && i != alphabeticNameIndex)
                                ++j;     // 'a' acts as 0 when it's the least significant digit, or 1 otherwise.
                            magnitude += j * _base26tobase10[alphabeticNameIndex - i];
                            if(magnitude < 0)                                                // Has the magnitude overflown and become negative?
                            {
                                magnitude = fraction ? int.MinValue : int.MaxValue;
                                return false;
                            }
                            break;
                        }
                    }
                }
                magnitude += _standardNames.Length;
                if(magnitude < 0)                                                            // Has the magnitude overflown and become negative?
                {
                    magnitude = fraction ? int.MinValue : int.MaxValue;
                    return false;
                }
                if(fraction)
                    magnitude *= -1;
                return true;
            }
        }


        /// <summary>
        /// Returns the alphabetic notation value as a "standard" double data type.
        /// </summary>
        /// <returns>The double type representation. Be aware the the result might be +- Infinity if the value overflows.</returns>
        /// <remarks>Not a particularly useful method.</remarks>
        public double Standard()
        {
            if(magnitude == 0)
                return coefficient;

            if(coefficient == 0)
                return 0d;

            return Math.Pow(1000, magnitude) * coefficient;
        }


        /// <summary>
        /// Because the string representation of this number could be displayed in so many different ways, this method is used
        /// mainly to get the most basic representation of a alphabetic notation number. Think debugging purposes.
        /// Type-Conversion (changing a numeric value to a string) will pretty much guarantee the production of garbage.
        /// </summary>
        public override string ToString()
        {
            var testMagnitude = magnitude;
            if(testMagnitude < 0)
                testMagnitude = -testMagnitude;
            if(testMagnitude == 0)
                return coefficient.ToString();
            if(testMagnitude == 1 && coefficient < 1)
                return (coefficient * 1000).ToString();
            _sb.Clear();
            _sb.ConvertAndAppend(coefficient);
            _sb.Append(GetAlphabeticMagnitudeName(magnitude));
            return _sb.ToString();
        }

        public string ToValuesString()
            => $"(c:{coefficient} m:{magnitude})";

        private static void Fix(ref double c, ref int m)
        {
            // Check to see if the new value has jumped an order of magnitude.
            var absCoefficient = c < 0 ? -c : c;

            if(absCoefficient == 0)
                return;

            while(absCoefficient < 0.01d) {
                --m;
                c *= 1000;
                absCoefficient *= 1000;
            }

            while(absCoefficient >= 1000d) {
                ++m;
                c /= 1000;
                absCoefficient /= 1000;
            }
        }



        // ---------------------------------------------------------------------------- Compare, Equatable

        /// <summary>
        /// Checks to see how this numbers compares to another numbers.
        /// </summary>
        /// <param name="other">The other numbers.</param>
        /// <returns>0 if the two numbers are the same*. -1 if this numbers smaller. 1 if this numbers is larger.</returns>
        public int CompareTo(AlphabeticNotation other)
        {
            // Difference in coefficient signs. Instant giveaway.
            if(other.coefficient <= 0 && coefficient > 0)
                return 1;
            if(other.coefficient >= 0 && coefficient < 0)
                return -1;

            // Same sign values, so now check the magnitudes.
            var magDiff = magnitude - other.magnitude;
            var thisCoefficient = coefficient;
            var otherCoefficient = other.coefficient;
            if(magDiff > 0) {
                if(magDiff >= _powers.Length)
                    thisCoefficient = double.MaxValue;
                else
                    thisCoefficient *= _powers[magDiff];
            }
            else if(magDiff < 0) {
                if(-magDiff >= _powers.Length)
                    otherCoefficient = double.MaxValue;
                else
                    otherCoefficient *= _powers[-magDiff];
            }

            if(thisCoefficient > otherCoefficient)
                return 1;
            if(thisCoefficient < otherCoefficient)
                return -1;
            return 0;
        }


        /// <summary>
        /// Determine if this number is the same as another.
        /// </summary>
        /// <param name="other">The other number.</param>
        /// <returns>True if the two numbers are the same to within three decimal places.</returns>
        public bool Equals(AlphabeticNotation other)
        {
            var magnitudeDifference = other.magnitude - magnitude;
            // Now performs an equality check with numbers up to 1 order of magnitude difference.
            if(magnitudeDifference < -1 || magnitudeDifference > 1)
                return false;
            //if ( other.magnitude != magnitude ) return false;
            var c1 = (int)(coefficient * 1_000 * (magnitudeDifference == -1 ? 1_000 : 1));
            var c2 = (int)(other.coefficient * 1_000 * (magnitudeDifference == 1 ? 1_000 : 1));
            return c1 == c2;
        }


        public override bool Equals(object obj)
            => obj is AlphabeticNotation other && Equals(other);

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + coefficient.GetHashCode();
                hash = hash * 23 + magnitude.GetHashCode();
                return hash;
            }
        }



        // ---------------------------------------------------------------------------- Operator Overloading
        public static AlphabeticNotation operator -(AlphabeticNotation a, AlphabeticNotation b)
        {
            var magnitudeDifference = a.magnitude - b.magnitude;
            var absmagnitudeDifference = magnitudeDifference < 0 ? -magnitudeDifference : magnitudeDifference;

            var returnCoefficient = a.coefficient;
            var returnMagnitude = a.magnitude;

            if(magnitudeDifference == 0) {
                // The magnitudes are the same, so we can just subtract the coefficients from each other.
                returnCoefficient -= b.coefficient;
            }
            else {
                if(magnitudeDifference < 0) {
                    // a is the smaller of the two numbers.
                    if(absmagnitudeDifference < _powers.Length)
                        returnCoefficient = (a.coefficient / _powers[absmagnitudeDifference]) - b.coefficient;
                    else
                        returnCoefficient = b.coefficient;
                    returnMagnitude = b.magnitude;
                }
                else {
                    // b is the smaller of the two numbers.
                    if(absmagnitudeDifference < _powers.Length)
                        returnCoefficient -= (b.coefficient / _powers[absmagnitudeDifference]);
                }
            }
            // If values less than 1.0 (e.g. 0.9) aren't wanted, then uncomment the call to Fix.
            //return Fix ( returnCoefficient, returnMagnitude );
            return new AlphabeticNotation(returnCoefficient, returnMagnitude);
        }

        public static AlphabeticNotation operator -(AlphabeticNotation a, double b)
            => a - new AlphabeticNotation(b);
        public static AlphabeticNotation operator -(double a, AlphabeticNotation b)
            => new AlphabeticNotation(a) - b;

        public static AlphabeticNotation operator +(AlphabeticNotation a, AlphabeticNotation b)
        {
            var magnitudeDifference = a.magnitude - b.magnitude;
            var absmagnitudeDifference = magnitudeDifference < 0 ? -magnitudeDifference : magnitudeDifference;

            var returnCoefficient = a.coefficient;
            var returnMagnitude = a.magnitude;

            if(magnitudeDifference == 0) {
                // The magnitudes are the same, so we can just add the values together.
                returnCoefficient += b.coefficient;
            }
            else {
                if(magnitudeDifference < 0) {
                    // a is the smaller of the two numbers.
                    if(absmagnitudeDifference < _powers.Length)
                        returnCoefficient = (a.coefficient / _powers[absmagnitudeDifference]) + b.coefficient;
                    else
                        returnCoefficient = b.coefficient;
                    returnMagnitude = b.magnitude;
                }
                else {
                    // b is the smaller of the two numbers.
                    if(absmagnitudeDifference < _powers.Length)
                        returnCoefficient += (b.coefficient / _powers[absmagnitudeDifference]);
                }
            }
            //return Fix ( returnCoefficient, returnMagnitude );
            return new AlphabeticNotation(returnCoefficient, returnMagnitude);
        }

        public static AlphabeticNotation operator +(AlphabeticNotation a, double b)
            => a + new AlphabeticNotation(b);
        public static AlphabeticNotation operator +(double a, AlphabeticNotation b)
            => new AlphabeticNotation(a) + b;

        public static AlphabeticNotation operator *(AlphabeticNotation a, AlphabeticNotation b)
            => new AlphabeticNotation(a.coefficient * b.coefficient, a.magnitude + b.magnitude);
        public static AlphabeticNotation operator /(AlphabeticNotation a, AlphabeticNotation b)
            => new AlphabeticNotation(a.coefficient / b.coefficient, a.magnitude - b.magnitude);
        public static AlphabeticNotation operator *(AlphabeticNotation a, double b)
            => new AlphabeticNotation(a.coefficient * b, a.magnitude);
        public static AlphabeticNotation operator /(AlphabeticNotation a, double b)
            => new AlphabeticNotation(a.coefficient / b, a.magnitude);
        public static AlphabeticNotation operator *(double a, AlphabeticNotation b)
            => new AlphabeticNotation(a * b.coefficient, b.magnitude);
        public static AlphabeticNotation operator /(double a, AlphabeticNotation b)
            => new AlphabeticNotation(a / b.coefficient, b.magnitude);

        public static bool operator <(AlphabeticNotation a, AlphabeticNotation b) => a.CompareTo(b) == -1;
        public static bool operator <=(AlphabeticNotation a, AlphabeticNotation b) => a.CompareTo(b) <= 0;
        public static bool operator >(AlphabeticNotation a, AlphabeticNotation b) => a.CompareTo(b) == 1;
        public static bool operator >=(AlphabeticNotation a, AlphabeticNotation b) => a.CompareTo(b) >= 0;

        public static bool operator ==(AlphabeticNotation a, AlphabeticNotation b) => a.Equals(b);
        public static bool operator !=(AlphabeticNotation a, AlphabeticNotation b) => !(a == b);


        public static implicit operator LargeNumber(AlphabeticNotation number)
            => new LargeNumber(number.coefficient, number.magnitude);

        public static implicit operator ScientificNotation(AlphabeticNotation number)
            => new ScientificNotation(number.coefficient, number.magnitude * 3);

        public static implicit operator double(AlphabeticNotation number)
        {
            if(number.magnitude == 0)
                return number.coefficient;

            if(number.coefficient == 0)
                return 0d;

            return Math.Pow(1000, number.magnitude) * number.coefficient;
        }
    }
}