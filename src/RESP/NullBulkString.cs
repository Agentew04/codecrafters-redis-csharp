using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP; 
public class NullBulkString : RespToken {

    public override RespToken FromRESP(string resp, out int endIndex) {
        endIndex = 0;
        return this;
    }

    public override string ToRESP() {
        return "$-1\r\n";
    }
}
