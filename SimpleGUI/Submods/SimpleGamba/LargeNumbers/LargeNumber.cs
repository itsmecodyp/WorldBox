// ----------------------------------------------------------------------------
// LargeNumbers.LargeNumber
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
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleGUI.Submods.SimpleGamba.LargeNumbers {
    /// <summary>
    /// A Conway & Guy Latin based number.
    /// </summary>
#if UNITY_2018_4_OR_NEWER
    [Serializable]
#endif
    public struct LargeNumber : IEquatable<LargeNumber> {
        internal readonly struct Prefix {
            internal readonly string Name;
            internal readonly UnitPrefixModifiers Modifier;

            internal Prefix(string Name, UnitPrefixModifiers Modifier)
            {
                this.Name = Name;
                this.Modifier = Modifier;
            }
        }


        // ---------------------------------------------------------------------------- Fields

        // This list helps reduce Maths.Pow calls, but also idicates the precision of mathematic operations. Values that are deemed too far different in scale are ignored.
        // I honestly can't believe any idle/clicker game is going to ever care about values this far apart.
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

        [Flags]
        internal enum UnitPrefixModifiers {
            None = 0,
            S = 1 << 0,
            X = 1 << 1,
            M = 1 << 2,
            N = 1 << 3,
            SX = S | X,
            SM = S | M,
            SN = S | N,
            XM = X | M,
            XN = X | N,
            MN = M | N
        }

        static internal readonly string[] _standardNames =
        {
            "",
            "Thousand",
            "Million",
            "Billion",
            "Trillion",
            "Quadrillion",
            "Quintillion",
            "Sextillion",
            "Septillion",
            "Octillion",
            "Nonillion",
            "Decillion",
        };

        static internal readonly Prefix[][] _prefixes =
        {
            // Units
            new[] {
                new Prefix ( string.Empty, UnitPrefixModifiers.None  ),
                new Prefix ( "Un", UnitPrefixModifiers.None  ),
                new Prefix ( "Duo", UnitPrefixModifiers.None  ),
                new Prefix ( "Tre", UnitPrefixModifiers.S  ),
                new Prefix ( "Quattuor", UnitPrefixModifiers.None  ),
                new Prefix ( "Quinqua", UnitPrefixModifiers.None  ),
                new Prefix ( "Se", UnitPrefixModifiers.SX  ),
                new Prefix ( "Septe", UnitPrefixModifiers.MN  ),
                new Prefix ( "Octo", UnitPrefixModifiers.None  ),
                new Prefix ( "Nove", UnitPrefixModifiers.MN  )
            },
            // Tens
            new[] {
                new Prefix ( string.Empty, UnitPrefixModifiers.None  ),
                new Prefix ( "Deci", UnitPrefixModifiers.N  ),
                new Prefix ( "Viginti", UnitPrefixModifiers.SM  ),
                new Prefix ( "Triginta", UnitPrefixModifiers.SN  ),
                new Prefix ( "Quadraginta", UnitPrefixModifiers.SN  ),
                new Prefix ( "Quinquaginta", UnitPrefixModifiers.SN  ),
                new Prefix ( "Sexaginta", UnitPrefixModifiers.N  ),
                new Prefix ( "Septuaginta", UnitPrefixModifiers.N  ),
                new Prefix ( "Octoginta", UnitPrefixModifiers.XM  ),
                new Prefix ( "Nonaginta", UnitPrefixModifiers.None  )
            },
            // Hundreds
            new[] {
                new Prefix ( string.Empty, UnitPrefixModifiers.None  ),
                new Prefix ( "Centi", UnitPrefixModifiers.XN  ),
                new Prefix ( "Ducenti", UnitPrefixModifiers.N  ),
                new Prefix ( "Trecenti", UnitPrefixModifiers.SN  ),
                new Prefix ( "Quadringenti", UnitPrefixModifiers.SN  ),
                new Prefix ( "Quingenti", UnitPrefixModifiers.SN  ),
                new Prefix ( "Sescenti", UnitPrefixModifiers.N  ),
                new Prefix ( "Septingenti", UnitPrefixModifiers.N  ),
                new Prefix ( "Octingenti", UnitPrefixModifiers.XM  ),
                new Prefix ( "Nongenti", UnitPrefixModifiers.None  )
            }
        };

        private static readonly StringBuilder _sb = new StringBuilder(44);
        private static readonly StringBuilder _largeNumberName = new StringBuilder(40);


        /// <summary>
        /// The coefficient is the value, or the number, that preceeds the latin name.
        /// </summary>
        public double coefficient;

        /// <summary>
        /// The magnitude refers to the short scale base (+1) of the large number. In scientic notation it is : coefficient x 10^(3 x magnitude).
        /// </summary>
        public int magnitude;

        public bool isZero => coefficient == 0;
        public static LargeNumber zero => new LargeNumber();

        // ---------------------------------------------------------------------------- Constructors
        /// <summary>
        /// Create a new LargeNumber using a double value as the starting value.
        /// </summary>
        /// <param name="value"></param>
        public LargeNumber(double value)
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
        /// Create a new LargeNumber.
        /// </summary>
        /// <param name="c">The coefficient for the new LargeNumber.</param>
        /// <param name="m">The magnitude for the new LargeNumber.</param>
        public LargeNumber(double c, int m)
        {
#if SKIPCHECKS
            if ( c == 0 )
#else
            if(c == 0 || double.IsInfinity(c) || double.IsNaN(c))
#endif
            {
                coefficient = 0;
                magnitude = 0;
                return;
            }

            Fix(ref c, ref m);
            coefficient = c;
            magnitude = m;
        }


        // ---------------------------------------------------------------------------- Methods

        /// <summary>
        /// This method allows the developer to get just the large number name (base name). It's especially helpful 
        /// when the value and name are going to be displayed differently in the UI.
        /// </summary>
        /// <param name="base">The short scale base value. Synonymous with magnitude,</param>
        public static string GetLargeNumberName(int @base)
        {
            if(@base >= 1001)
                return "Humongous!";
            if(@base <= -1001)
                return "Humongouth!";
            var absBase = @base < 0 ? -@base : @base;
            if(absBase < 2)
                return string.Empty;

            _largeNumberName.Clear();

            if(absBase < _standardNames.Length) {
                _largeNumberName.Append(_standardNames[absBase]);
                if(@base < 0)
                    _largeNumberName.Append("th");
                return _largeNumberName.ToString();
            }

            --absBase;
            var hundreds = absBase / 100;
            var newBase = absBase - (hundreds * 100);
            var tens = newBase / 10;
            newBase -= (tens * 10); // is now the units

            var unitsModifier = UnitPrefixModifiers.None;
            if(newBase != 0) {
                unitsModifier = _prefixes[0][newBase].Modifier;
                _largeNumberName.Append(_prefixes[0][newBase].Name);
            }

            if(tens > 0) {
                if(unitsModifier != UnitPrefixModifiers.None) {
                    var modiferIntersection = unitsModifier & _prefixes[1][tens].Modifier;
                    switch(modiferIntersection) {
                        case UnitPrefixModifiers.S:
                            _largeNumberName.Append("s");
                            break;
                        case UnitPrefixModifiers.X:
                            _largeNumberName.Append("x");
                            break;
                        case UnitPrefixModifiers.M:
                            _largeNumberName.Append("m");
                            break;
                        case UnitPrefixModifiers.N:
                            _largeNumberName.Append("n");
                            break;
                    }
                    unitsModifier = UnitPrefixModifiers.None;
                }
                _largeNumberName.Append(_prefixes[1][tens].Name);
            }

            if(hundreds > 0) {
                if(unitsModifier != UnitPrefixModifiers.None) {
                    var modiferIntersection = unitsModifier & _prefixes[2][hundreds].Modifier;
                    switch(modiferIntersection) {
                        case UnitPrefixModifiers.S:
                            _largeNumberName.Append("s");
                            break;
                        case UnitPrefixModifiers.X:
                            _largeNumberName.Append("x");
                            break;
                        case UnitPrefixModifiers.M:
                            _largeNumberName.Append("m");
                            break;
                        case UnitPrefixModifiers.N:
                            _largeNumberName.Append("n");
                            break;
                    }
                }
                _largeNumberName.Append(_prefixes[2][hundreds].Name);
            }
            _largeNumberName.Remove(_largeNumberName.Length - 1, 1);
            _largeNumberName.Append("illion");

            if(@base < 0)
                _largeNumberName.Append("th");
            return _largeNumberName.ToString();
        }


        /// <summary>
        /// Returns the LargeNumber value as a "standard" double data type.
        /// </summary>
        /// <returns>The double type representation. Be aware the the result might be +- Infinity if the value overflows.</returns>
        /// <remarks>Not a particularly useful method.</remarks>
        public double Standard()
        {
            if(magnitude == 0)
                return coefficient;

            if(coefficient == 0)
                return 0d;

            return Math.Pow(10, 3 * magnitude) * coefficient;
        }


        /// <summary>
        /// Because the string representation of this number could be displayed in so many different ways, this method is used
        /// mainly to get the most basic representation of a Conway & Guy latin based number. Think debugging purposes.
        /// Type-Conversion (changing a numeric value to a string) will pretty much guarantee the production of garbage.
        /// </summary>
        public override string ToString()
        {
            _sb.Clear();
            var absMagnitude = magnitude < 0 ? -magnitude : magnitude;
            if(absMagnitude < 2) {
                if(magnitude > 0) {
                    _sb.ConvertAndAppend(coefficient * _powers[magnitude], 3, false);
                    return _sb.ToString();
                }

                _sb.ConvertAndAppend(coefficient / _powers[-magnitude], 3, false);
                return _sb.ToString();
            }

            _sb.ConvertAndAppend(coefficient, 0, false).Append(' ');
            _sb.Append(GetLargeNumberName(magnitude));
            return _sb.ToString();
        }

        public string ToValuesString() => $"(c:{coefficient} m:{magnitude})";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LargeNumber Fix(double c, int m)
        {
            // Check to see if the new value has jumped an order of magnitude.
            var absCoefficient = c < 0 ? -c : c;

            if(absCoefficient == 0)
                return new LargeNumber(0d, 0);

            while(absCoefficient < 1d) {
                --m;
                c *= 1000;
                absCoefficient *= 1000;
            }

            while(absCoefficient >= 1000d) {
                ++m;
                c /= 1000;
                absCoefficient /= 1000;
            }
            return new LargeNumber(c, m);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fix(ref double c, ref int m)
        {
            // Check to see if the new value has jumped an order of magnitude.
            var absCoefficient = c < 0 ? -c : c;

            if(absCoefficient == 0)
                return;


            // e.g. c:0.00999, m:1 -> c:9.99, m:0.
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
        public int CompareTo(LargeNumber other)
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
        /// <returns>True if the two number are the same to within three decimal places.</returns>
        public bool Equals(LargeNumber other)
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


        public override bool Equals(object obj) => obj is LargeNumber other && Equals(other);


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

        public static LargeNumber operator +(LargeNumber a, LargeNumber b)
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
                        returnCoefficient = (returnCoefficient / _powers[absmagnitudeDifference]) + b.coefficient;
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
            return new LargeNumber(returnCoefficient, returnMagnitude);
        }


        public static LargeNumber operator -(LargeNumber a, LargeNumber b)
        {
            var magnitudeDifference = a.magnitude - b.magnitude;
            var absmagnitudeDifference = magnitudeDifference < 0 ? -magnitudeDifference : magnitudeDifference;

            var returnCoefficient = a.coefficient;
            var returnMagnitude = a.magnitude;

            if(magnitudeDifference == 0) {
                // The magnitudes are the same, so we can just subtract the values together.
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
            return new LargeNumber(returnCoefficient, returnMagnitude);
        }


        public static LargeNumber operator +(LargeNumber a, double b)
            => a + new LargeNumber(b);
        public static LargeNumber operator +(double a, LargeNumber b)
            => new LargeNumber(a) + b;
        public static LargeNumber operator -(LargeNumber a, double b)
            => a - new LargeNumber(b);
        public static LargeNumber operator -(double a, LargeNumber b)
            => new LargeNumber(a) - b;

        public static LargeNumber operator *(LargeNumber a, LargeNumber b)
            => new LargeNumber(a.coefficient * b.coefficient, a.magnitude + b.magnitude);
        public static LargeNumber operator *(LargeNumber a, double b)
            => new LargeNumber(a.coefficient * b, a.magnitude);
        public static LargeNumber operator *(double a, LargeNumber b)
            => new LargeNumber(a * b.coefficient, b.magnitude);
        public static LargeNumber operator /(LargeNumber a, LargeNumber b)
            => new LargeNumber(a.coefficient / b.coefficient, a.magnitude - b.magnitude);
        public static LargeNumber operator /(LargeNumber a, double b)
            => new LargeNumber(a.coefficient / b, a.magnitude);
        public static LargeNumber operator /(double a, LargeNumber b)
            => new LargeNumber(a / b.coefficient, b.magnitude);

        public static bool operator <(LargeNumber a, LargeNumber b)
            => a.CompareTo(b) == -1;
        public static bool operator <=(LargeNumber a, LargeNumber b)
            => a.CompareTo(b) <= 0;
        public static bool operator >(LargeNumber a, LargeNumber b)
            => a.CompareTo(b) == 1;
        public static bool operator >=(LargeNumber a, LargeNumber b)
            => a.CompareTo(b) >= 0;

        public static bool operator ==(LargeNumber a, LargeNumber b)
            => a.Equals(b);
        public static bool operator !=(LargeNumber a, LargeNumber b)
            => !a.Equals(b);

        public static implicit operator ScientificNotation(LargeNumber number)
            => new ScientificNotation(number.coefficient, number.magnitude * 3);
        public static implicit operator AlphabeticNotation(LargeNumber number)
            => new AlphabeticNotation(number.coefficient, number.magnitude);

        public static implicit operator double(LargeNumber number)
        {
            if(number.magnitude == 0)
                return number.coefficient;

            if(number.coefficient == 0)
                return 0d;

            return Math.Pow(1000, number.magnitude) * number.coefficient;
        }
    }
}