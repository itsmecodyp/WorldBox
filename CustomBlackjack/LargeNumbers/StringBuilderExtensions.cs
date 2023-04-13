// ----------------------------------------------------------------------------
// LargeNumbers.StringBuilderExtensions
// (c) Milan Egon Votrubec
//
// For documentation, please see the included "Large Numbers Library Welcome.pdf" file.
//
// For example usage, please see the "largenumbers.unity" example scene.
//
// Please see the "license.txt" file for licensing.
// ----------------------------------------------------------------------------

using System.Text;


namespace LargeNumbers {
    public static partial class StringBuilderExtensions {
        private static readonly long[] _powers = {
            1,
            10,
            100,
            1_000,
            10_000,
            100_000,
            1_000_000,
            10_000_000,
            100_000_000,
            1_000_000_000,
            10_000_000_000,
            100_000_000_000,
            1_000_000_000_000,
            10_000_000_000_000,
            100_000_000_000_000,
            1_000_000_000_000_000
        };

        private static readonly char[] _characters = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };


        // ---------------------------------------------------------------------------- Methods
        /// <summary>
        /// ConvertAndAppend is a low garbage producing int to string converter.
        /// </summary>
        /// <param name="value">The int type value to convert to a string.</param>
        /// <returns>The string representation of the int type.</returns>
        public static StringBuilder ConvertAndAppend(this StringBuilder sb, int value)
        {
            if(value < 0) {
                value = -value;
                sb.Append('-');
            }

            var intLength = 0;
            var intPart = (uint)value;
            var temp = intPart;

            do {
                temp /= 10;
                intLength++;
            }
            while(temp > 0);

            sb.Append('0', intLength);
            var currentPosition = sb.Length - 1;
            var currentCount = intLength;
            do {
                sb[currentPosition] = _characters[intPart % 10];
                intPart /= 10;
                currentPosition--;
                currentCount--;
            }
            while(currentCount > 0);
            return sb;
        }


        /// <summary>
        /// ConvertAndAppend is a low garbage producing double to string converter.
        /// </summary>
        /// <param name="value">The double type vlue to convert to a string.</param>
        /// <param name="decimalPlaces">How many decimal places to display. Defaults to 3.</param>
        /// <param name="padDecimalWithZeros">Should the deciaml places be padded with 0's. Defaults to true.</param>
        /// <returns>The string representation of the double type.</returns>
        public static StringBuilder ConvertAndAppend(this StringBuilder sb, double value, int decimalPlaces = 3, bool padDecimalWithZeros = true)
        {
            var intPart = (int)value;
            var decimalPart = (int)((value - intPart) * _powers[decimalPlaces]);
            if(decimalPart < 0) decimalPart = -decimalPart;

            // Build the whole part
            if(intPart < 0) intPart = -intPart;
            if(value < 0) sb.Append("-");

            sb.ConvertAndAppend(intPart);
            if(decimalPart == 0)
                return sb;

            // Build the decimal part
            var temp = decimalPart;
            var decimalLength = 0;
            while(temp > 0) {
                temp /= 10;
                decimalLength++;
            }
            sb.Append('.');
            if(decimalLength < decimalPlaces)
                sb.Append('0', decimalPlaces - decimalLength);
            sb.ConvertAndAppend(decimalPart);

            if(!padDecimalWithZeros) {
                var currentPosition = sb.Length;
                while(sb[--currentPosition] == '0') ;
                sb.Length = currentPosition + 1;
            }
            return sb;
        }


        /// <summary>
        /// ConvertAndAppend is a low garbage producing double to string converter.
        /// </summary>
        /// <param name="value">The double type value to convert to a string.</param>
        /// <param name="totalNumerals">How many decimal places to display. Defaults to 3.</param>
        /// <returns>The string representation of the double type. Values are rounded down.</returns>
        public static StringBuilder ConvertAndAppendTruncated(this StringBuilder sb, double value, int totalNumerals = 3, bool forceDecimal = false)
        {
            var intPart = (int)value;

            sb.ConvertAndAppend(intPart);
            var wholeNumberSize = sb.Length;
            if(intPart < 0) --wholeNumberSize;
            if(wholeNumberSize >= totalNumerals) return sb;

            // Build the decimal part
            var decimalPlaces = totalNumerals - wholeNumberSize;

            var decimalPart = (int)((value - intPart) * _powers[decimalPlaces]);
            if(decimalPart == 0) {
                if(forceDecimal) {
                    sb.Append('.');
                    sb.Append('0', decimalPlaces);
                }
                return sb;
            }
            if(decimalPart < 0)
                decimalPart = -decimalPart;

            var temp = decimalPart;
            var decimalLength = 0;
            while(temp > 0 && decimalLength < decimalPlaces) {
                temp /= 10;
                decimalLength++;
            }

            sb.Append('.');
            if(decimalLength < decimalPlaces)
                sb.Append('0', decimalPlaces - decimalLength);
            sb.ConvertAndAppend(decimalPart);
            return sb;
        }

    }
}