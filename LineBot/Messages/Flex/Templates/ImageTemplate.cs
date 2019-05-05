using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Line.Messages.Flex.Templates {
    public class ImageTemplate : IBubbleTemplate {
#pragma warning disable 0414 // Suppress value is never used.
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter<BubbleTemplateType>))]
        private readonly BubbleTemplateType _type = BubbleTemplateType.Box;
#pragma warning restore 0414

    }
}
