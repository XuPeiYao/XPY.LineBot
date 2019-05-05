﻿// Copyright 2017-2019 Dirk Lemstra (https://github.com/dlemstra/line-bot-sdk-dotnet)
//
// Dirk Lemstra licenses this file to you under the Apache License,
// version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at:
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations
// under the License.

namespace Line {
    /// <summary>
    /// Encapsulates the message types.
    /// </summary>
    public enum MessageType {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Audio message.
        /// </summary>
        Audio,

        /// <summary>
        /// Image message.
        /// </summary>
        Image,

        /// <summary>
        /// Imagemap message.
        /// </summary>
        Imagemap,

        /// <summary>
        /// Location message.
        /// </summary>
        Location,

        /// <summary>
        /// Sticker message.
        /// </summary>
        Sticker,

        /// <summary>
        /// Template message.
        /// </summary>
        Template,

        /// <summary>
        /// Text message.
        /// </summary>
        Text,

        /// <summary>
        /// Video message.
        /// </summary>
        Video,

        /// <summary>
        /// Flex message
        /// </summary>
        Flex,
    }
}
