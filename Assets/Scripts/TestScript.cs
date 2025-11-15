using System.Collections.Generic;
using System.Linq;
using OpenAI;
using OpenAI.Files;
using OpenAI.Responses;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        Debug.Log(new OpenAIAuthentication().LoadFromDirectory(Application.dataPath + "/.openai").Info.ApiKey);
        var api = new OpenAIClient(new OpenAIAuthentication().LoadFromDirectory(Application.dataPath + "/.openai"));
        var file = await api.FilesEndpoint.UploadFileAsync(
            Application.dataPath + "/living-room-sample.png",
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
