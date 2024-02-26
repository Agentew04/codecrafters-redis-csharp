using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src.RESP {
    public class ArrayToken : RespToken {
        public int Count { get; set; }
        public List<RespToken> Tokens { get; set; } = new();

        public override RespToken FromRESP(string resp, out int endIndex) {
            Count = ReadSize(resp, 1, out int endIdx);
            int dataIndex = endIdx + 2;
            Tokens = new List<RespToken>();
            for (int i = 0; i < Count; i++) {
                var token = RespToken.Parse(resp[dataIndex..], out int tokenEndIdx);
                Tokens.Add(token);
                dataIndex += tokenEndIdx;
            }
            endIndex = endIdx + 2 + Count;
            return this;
        }

        public override string ToRESP() {
            StringBuilder sb = new();

            sb.Append($"*{Count}\r\n");
            foreach (var token in Tokens) {
                sb.Append(token.ToRESP());
            }

            return sb.ToString();
        }
    }
}
