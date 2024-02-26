using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP {
    public class ArrayToken : RespToken {
        public int Count { get; set; }
        public List<RespToken> Tokens { get; set; } = new();

        public override RespToken FromRESP(byte[] resp, out int endIndex) {
            Count = ReadSize(resp, 1, out int endIdx);
            int dataIndex = endIdx + 2;
            Tokens = new List<RespToken>();
            for (int i = 0; i < Count; i++) {
                var token = Parse(resp[dataIndex..], out int tokenEndIdx);
                Tokens.Add(token);
                dataIndex += tokenEndIdx;
            }
            endIndex = endIdx + 2 + Count;
            return this;
        }

        public override byte[] ToRESP() {
            List<byte> bytes = new();

            bytes.AddRange($"*{Count}\r\n".ToAscii());
            foreach (var token in Tokens) {
                bytes.AddRange(token.ToRESP());
            }

            return bytes.ToArray();
        }
    }
}
