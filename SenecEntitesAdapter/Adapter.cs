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

        public SenecDecimal GetDecimal(string data)
        {
            return GetValue(data) as SenecDecimal;
        }

        public SenecValue GetValue(string data)
        {
            var tmp = data.Split('_');
            var value = new SenecString(SenecString.Unknown, tmp[1]);
            switch (tmp[0])
            {
                case "fl":
                    return ToFloat(tmp[1]);
                case "u8":
                    return ToInteger(SenecDecimal.Unsigned8, value);
                case "u1":
                    return ToInteger(SenecDecimal.Unsigned1, value);
                case "u3":
                    return ToInteger(SenecDecimal.Unsigned3, value);
                case "u6":
                    return ToInteger(SenecDecimal.Unsigned6, value);
                case "i1":
                    return ToInteger(SenecDecimal.Integer1, value, 0x8000, 0x10000);
                case "i3":
                    return ToInteger(SenecDecimal.Integer3, value, 0x80000000, 0x100000000);
                case "i8":
                    return ToInteger(SenecDecimal.Integer8, value, 0x80, 0x100);
                case "ch":
                    value.Type = SenecString.Character;
                    break;
                case "st":
                    value.Type = SenecString.String;
                    break;
                case "er":
                    value.Type = SenecString.Error;
                    break;
            }
            return value;
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
