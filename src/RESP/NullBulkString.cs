using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class NullBulkString : RespToken {

    public override RespToken FromRESP(byte[] resp, out int endIndex) {
        if (resp[0] != '$' || resp[1] != '-' || resp[2] != '1' || resp[3] != '\r' || resp[4] != '\n') {
            throw new InvalidDataException("Invalid RESP data");
        }
        endIndex = 5;
        return this;
    }

    public override byte[] ToRESP() {
        return "$-1\r\n".ToAscii();
    }
}
