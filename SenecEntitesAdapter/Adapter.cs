using Entities;
using System;
using System.Globalization;

namespace SenecEntitiesAdapter
{
    public class Adapter : IAdapter
    {
        private double hex2float(long num)
        {
            var sign = (num & 0x80000000) > 0 ? -1 : 1;
            var exponent = ((num >> 23) & 0xff) - 127;
            var mantissa = Convert.ToString((num & 0x7fffff) + 0x800000, 2);
            var result = 0.0;

            for (var i=0; i<mantissa.Length; i+=1)
            {
                result += mantissa[i] == '1' ? Math.Pow(2,exponent) : 0;
                exponent--;
            }
            return sign * result;
        }

        private long? _hex2long(string number)
        {
            if (long.TryParse(number, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat, out long result))
                return result;
            return null;
        }

        public SenecDecimal GetDecimal(string? data)
        {
            var value = GetValue(data);
            if (value is SenecDecimal decimalValue)
                return decimalValue;
            return new SenecDecimal(value.Type, null);
        }

        public SenecValue GetValue(string? data)
        {
            if (data == null)
                return new SenecString(SenecString.Unknown, "");
            var valueArray = data.Split('_');
            if (valueArray.Length != 2)
                return new SenecString(SenecString.Unknown, data);

            var value = valueArray[1];
            return (valueArray[0]) switch
            {
                "fl" => ToFloat(valueArray[1]),
                "u8" => ToInteger(SenecDecimal.Unsigned8, value),
                "u1" => ToInteger(SenecDecimal.Unsigned1, value),
                "u3" => ToInteger(SenecDecimal.Unsigned3, value),
                "u6" => ToInteger(SenecDecimal.Unsigned6, value),
                "i1" => ToInteger(SenecDecimal.Integer1, value, 0x8000, 0x10000),
                "i3" => ToInteger(SenecDecimal.Integer3, value, 0x80000000, 0x100000000),
                "i8" => ToInteger(SenecDecimal.Integer8, value, 0x80, 0x100),
                "ch" => new SenecString(SenecString.Character, value),
                "st" => new SenecString(SenecString.String, value),
                "er" => new SenecString(SenecString.Error, value),
                _ => new SenecString(SenecString.Unknown, data),
            };
        }

        private SenecValue ToInteger(int type, string valueText)
        {
            var result = _hex2long(valueText);
            var response = new SenecDecimal(type);
            if (result != null)
                response.Value = result.Value;
            return response;
        }

        private SenecValue ToInteger(int type, string valueText, long ceiling, long floor)
        {
            var result = _hex2long(valueText);
            var response = new SenecDecimal(type);
            if (result == null) return response;
            if ((result & ceiling) > 0)
            {
                response.Value = result.Value - floor;
            }
            else
            {
                response.Value = result;
            }
            return response;
        }

        private SenecValue ToFloat(string valueText)
        {
            var value = _hex2long(valueText);
            if (value == null) return new SenecDecimal(SenecDecimal.Float, null);
            var result = hex2float(value.Value);
            var roundResult = (decimal)Math.Round(result, 2);
            return new SenecDecimal(SenecDecimal.Float, roundResult);
        }
    }
}
