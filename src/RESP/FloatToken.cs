using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class FloatToken : RespToken {

    public float Value { get; set; }

    public override byte[] ToRESP() {
        List<byte> bytes = new();
        bytes.AddRange(",".ToAscii());
        string valueStr;
        if (float.IsPositiveInfinity(Value)) {
            valueStr = "inf";
        }else if (float.IsNegativeInfinity(Value)) {
            valueStr = "-inf";
        }else if (float.IsNaN(Value)) {
            valueStr = "nan";
        } else {
            valueStr = Value.ToString();
        }
        bytes.AddRange(valueStr.ToAscii());
        bytes.AddRange("\r\n".ToAscii());
        return bytes.ToArray();
    }

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        int i = 1;
        int start = i;
        while (resp[i] != '\r' && resp[i + 1] != '\n') {
            i++;
        }
        string number = resp[start..i].FromAscii();
        if(number == "inf")
            Value = float.PositiveInfinity;
        else if(number == "-inf")
            Value = float.NegativeInfinity;
        else if(number == "nan")
            Value = float.NaN;
        else
            Value = float.Parse(number);
        endIndex = i + 2;
        return this;
    }
}
