using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class FloatToken : RespToken {

    public float Value { get; set; }

    public override string ToRESP() {
        return $",{Value}\r\n";
    }

    public override RespToken FromRESP(string resp, out int endIndex) {
        int i = 1;
        int start = i;
        while (resp[i] != '\r' && resp[i + 1] != '\n') {
            i++;
        }
        string number = resp.Substring(start, i - start);
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
