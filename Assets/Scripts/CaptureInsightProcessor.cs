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
    [SerializeField] private OpenAIConfiguration openAIConfig;
    [Button(30)]
    public async Task<string> FetchCaptureMusicInsights()
    {
        var api = new OpenAIClient(new OpenAIAuthentication(openAIConfig), new OpenAISettings(openAIConfig));
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
                    new OpenAI.Responses.TextContent("Analyze the space’s mood, lighting, textures, and season to guess what activity the user might be doing. Use this to create an instrumental ambient music prompt. Describe the atmosphere vividly, then suggest a flexible genre, instruments, and any subtle nature sounds or special effects. Keep the generated text concise (under 499 characters) and ensure it’s dynamic, immersive, and fitting to the space’s vibe."),
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