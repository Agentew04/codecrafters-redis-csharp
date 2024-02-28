using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
internal class SimpleStringToken : RespToken {

    public string Value { get; set; } = "";

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        int index = 1;
        while (resp[index] != '\r') {
            index++;
        }
        Value = resp[1..index].FromAscii();
        endIndex = index + 2;
        return this;
    }

    public override byte[] ToRESP() {
        return $"+{Value}\r\n".ToAscii();
    }
}
