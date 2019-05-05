using Line.Messages.Flex.Templates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Line.Messages.Flex {
    public class BubbleTemplate {
#pragma warning disable 0414 // Suppress value is never used.
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter<BubbleTemplateType>))]
        private readonly BubbleTemplateType _type = BubbleTemplateType.Bubble;
#pragma warning restore 0414

        /// <summary>
        /// 標頭
        /// </summary>
        [JsonProperty("header")]
        public IBubbleTemplate Header { get; set; }

        /// <summary>
        /// 主圖
        /// </summary>
        [JsonProperty("hero")]
        public ImageTemplate Hero { get; set; }

        /// <summary>
        /// 本文
        /// </summary>
        [JsonProperty("body")]
        public IBubbleTemplate Body { get; set; }

        /// <summary>
        /// 尾端
        /// </summary>
        [JsonProperty("footer")]
        public IBubbleTemplate Footer { get; set; }

        public void Validate() {
            if (Header == null &&
                Hero == null &&
                Body == null &&
                Footer == null) {
                throw new InvalidOperationException("Have at least one property");

            }
        }
    }
}
