/*
   Copyright 2026 CrypTool Project

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.CrypLLM.Threads
{
    /// <summary>
    /// Converts text and tool messages into a version-tolerant storage format.
    /// Explicit item kinds preserve call/result pairing without deserializing arbitrary CLR types.
    /// </summary>
    internal static class ChatHistoryPersistence
    {
        /// <summary>
        /// Retains structured tool messages, including messages whose Content property is null.
        /// Synthetic greetings are UI state and are not part of the persisted conversation.
        /// </summary>
        internal static List<PersistedChatMessage> SerializeMessages(IEnumerable<ChatMessageContent> messages)
        {
            return (messages ?? Enumerable.Empty<ChatMessageContent>())
                .Where(message => message != null && !AIThread.IsSyntheticGreetingMessage(message))
                .Select(ToDto)
                .Where(message => !string.IsNullOrWhiteSpace(message.Content) || message.Items.Count > 0)
                .ToList();
        }

        /// <summary>
        /// Restores structured messages and accepts legacy records containing only Role and Content.
        /// </summary>
        internal static List<ChatMessageContent> DeserializeMessages(IEnumerable<PersistedChatMessage> messages)
        {
            return (messages ?? Enumerable.Empty<PersistedChatMessage>())
                .Where(message => message != null)
                .Select(FromDto)
                .ToList();
        }

        /// <summary>Stores text, tool identity, arguments and results in their original item order.</summary>
        private static PersistedChatMessage ToDto(ChatMessageContent message)
        {
            var dto = new PersistedChatMessage
            {
                Role = message.Role.ToString(),
                Content = message.Content,
                AuthorName = message.AuthorName,
                ModelId = message.ModelId
            };
            foreach (KernelContent item in message.Items)
            {
                if (item is TextContent text)
                {
                    dto.Items.Add(new PersistedChatItem { Kind = "text", Text = text.Text });
                }
                else if (item is FunctionCallContent call)
                {
                    dto.Items.Add(new PersistedChatItem
                    {
                        Kind = "call", Id = call.Id, PluginName = call.PluginName,
                        FunctionName = call.FunctionName,
                        Arguments = call.Arguments == null ? null : JObject.FromObject(call.Arguments)
                    });
                }
                else if (item is FunctionResultContent result)
                {
                    dto.Items.Add(new PersistedChatItem
                    {
                        Kind = "result", Id = result.CallId, PluginName = result.PluginName,
                        FunctionName = result.FunctionName, Result = SerializeResult(result.Result)
                    });
                }
            }
            return dto;
        }

        /// <summary>Recreates Semantic Kernel items without duplicating legacy Content text.</summary>
        private static ChatMessageContent FromDto(PersistedChatMessage dto)
        {
            var message = new ChatMessageContent(new AuthorRole(dto.Role ?? "assistant"), (string)null)
            {
                AuthorName = dto.AuthorName,
                ModelId = dto.ModelId
            };
            foreach (PersistedChatItem item in dto.Items ?? new List<PersistedChatItem>())
            {
                if (item == null) continue;
                switch (item.Kind)
                {
                    case "text":
                        message.Items.Add(new TextContent(item.Text ?? string.Empty));
                        break;
                    case "call":
                        var arguments = item.Arguments?.ToObject<Dictionary<string, object>>();
                        message.Items.Add(new FunctionCallContent(item.Id, item.PluginName,
                            item.FunctionName, arguments == null ? null : new KernelArguments(arguments)));
                        break;
                    case "result":
                        message.Items.Add(new FunctionResultContent(item.Id, item.PluginName,
                            item.FunctionName, item.Result?.ToObject<object>()));
                        break;
                }
            }
            // Old records have no Items; avoid adding duplicate text to current records.
            if (message.Items.Count == 0 && dto.Content != null)
            {
                message.Content = dto.Content;
            }
            return message;
        }

        /// <summary>Preserves JSON-compatible results and reduces exceptions to their message.</summary>
        private static JToken SerializeResult(object result)
        {
            if (result == null) return JValue.CreateNull();
            if (result is Exception exception) return new JValue(exception.Message);
            try { return JToken.FromObject(result); }
            catch { return new JValue(result.ToString()); }
        }
    }

    /// <summary>Storage record compatible with the original Role/Content-only format.</summary>
    internal sealed class PersistedChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string ModelId { get; set; }
        public List<PersistedChatItem> Items { get; set; } = new List<PersistedChatItem>();
    }

    /// <summary>Discriminated text, function-call or function-result payload.</summary>
    internal sealed class PersistedChatItem
    {
        public string Kind { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
        public string PluginName { get; set; }
        public string FunctionName { get; set; }
        public JObject Arguments { get; set; }
        public JToken Result { get; set; }
    }
}
