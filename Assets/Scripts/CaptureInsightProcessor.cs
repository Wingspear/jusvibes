using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Files;
using OpenAI.Responses;
using Sirenix.OdinInspector;
using UnityEngine;

public class CaptureInsightProcessor : MonoBehaviour
{
    [Button(30)]
    public async Task<string> FetchCaptureMusicInsights()
    {
        Debug.Log(new OpenAIAuthentication().LoadFromDirectory(Application.streamingAssetsPath + "/.openai").Info.ApiKey);
        var api = new OpenAIClient(new OpenAIAuthentication().LoadFromDirectory(Application.dataPath + "/.openai"));
        var file = await api.FilesEndpoint.UploadFileAsync(
            Application.persistentDataPath + "/capture.png",
            FilePurpose.Vision
        );

        var input = new List<IResponseItem>
        {
            new Message(
                Role.User,
                new IResponseContent[]
                {
                    new OpenAI.Responses.TextContent("Analyze this room visually and describe its atmosphere in a way that can be used as a prompt for ambient music generation. Focus on the mood, lighting, textures, and the emotional character of the space. Produce a concise, vivid music prompt that fits the room’s aesthetic.\n"),
                    new OpenAI.Responses.ImageContent(fileId: file.Id),
                }
            )
        };

        var request = new CreateResponseRequest(
            input: input,
            model: "gpt-4.1-mini"
        );

        var response = await api.ResponsesEndpoint.CreateModelResponseAsync(request);
        var responseItem = response.Output.LastOrDefault();

        if (responseItem != null)
        {
            Debug.Log(responseItem.ToString());
            response.PrintUsage();
        }

        return response;
    }
}