using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP;
public class BulkStringToken : RespToken {

    public int Length { get; set; }
    public string Value { get; set; } = "";

    public override RespToken FromRESP(string resp, out int endIndex) {
        int sizeEndIndex = 1;
        // read string numbers until \r\n
        while (resp[sizeEndIndex] >= '0' && resp[sizeEndIndex] <= '9') {
            sizeEndIndex++;
        }

        string sizeStr = resp[1..sizeEndIndex];
        Length = int.Parse(sizeStr);
        Value = resp.Substring(sizeEndIndex + 2, Length);
        endIndex = sizeEndIndex + 2 + Length + 2;
        return this;
    }

    public override string ToRESP() {
        return $"${Length}\r\n{Value}\r\n";
    }

    public static BulkStringToken FromString(string value) {
        return new BulkStringToken() {
            Length = value.Length,
            Value = value
        };
    }
}
