using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Line.Messages.Flex {
    public class FlexMessage : ISendMessage {
#pragma warning disable 0414 // Suppress value is never used.
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter<MessageType>))]
        private readonly MessageType _type = MessageType.Flex;
#pragma warning restore 0414

        private string _alternativeText;

        /// <summary>
        /// Gets or sets the alternative text for devices that do not support this type of message.
        /// <para>Max: 400 characters.</para>
        /// </summary>
        [JsonProperty("altText")]
        public string AlternativeText {
            get {
                return _alternativeText;
            }

            set {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException("The alternative text cannot be null or whitespace.");

                if (value.Length > 400)
                    throw new InvalidOperationException("The alternative text cannot be longer than 400 characters.");

                _alternativeText = value;
            }
        }

        [JsonProperty("contents")]
        public BubbleTemplate Contents { get; set; }


        public void Validate() {
            if (_alternativeText == null)
                throw new InvalidOperationException("The alternative text cannot be null.");

            if (Contents == null)
                throw new InvalidOperationException("The contents cannot be null.");

            Contents.Validate();
        }
    }
}
