using System.Text.Json;
using TelegramAi.Application.DTOs;

namespace TelegramAi.Infrastructure.Services
{
    public class AssistantResponseParser
    {
        public AssistantParsedResult Parse(string assistantResponse)
        {
            if (assistantResponse == null) throw new ArgumentNullException(nameof(assistantResponse));

            var jsonPosts = new List<string>();
            int searchIndex = 0;

            // Find all /publish JSON blocks
            while (true)
            {
                var publishIndex = assistantResponse.IndexOf("/publish", searchIndex, StringComparison.OrdinalIgnoreCase);
                if (publishIndex < 0) break;

                var braceIndex = assistantResponse.IndexOf('{', publishIndex);
                if (braceIndex < 0) break;

                int depth = 0;
                int endIndex = -1;
                for (int i = braceIndex; i < assistantResponse.Length; i++)
                {
                    if (assistantResponse[i] == '{') depth++;
                    if (assistantResponse[i] == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }

                if (endIndex < 0) break;

                // Extract JSON substring
                var json = assistantResponse.Substring(braceIndex, endIndex - braceIndex + 1).Trim();
                jsonPosts.Add(json);

                searchIndex = endIndex + 1;
            }

            // Remove all /publish blocks from text
            var cleaned = assistantResponse;
            foreach (var json in jsonPosts)
            {
                var marker = $"/publish {json}";
                cleaned = cleaned.Replace(marker, "", StringComparison.OrdinalIgnoreCase);
            }

            cleaned = cleaned.Trim();

            return new AssistantParsedResult
            {
                Text = cleaned,
                JsonPosts = jsonPosts.SelectMany<string, ChannelPostDto>(x =>
                {
                    var result = JsonSerializer.Deserialize<ChannelPostDto>(x);
                    return result is null ? [] : [result];
                }).ToList()
            };
        }
    }

    public class AssistantParsedResult
    {
        public string Text { get; set; } = null!;
        public List<ChannelPostDto> JsonPosts { get; set; } = [];
    }
}
