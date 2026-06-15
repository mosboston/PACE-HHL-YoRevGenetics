using FAST;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

using Application = FAST.Application;

public class ProteinLogicLoader : StartupLoader
{
    private string proteinLogicPath;

    protected override IEnumerator ExecuteLoad()
    {
        proteinLogicPath = Path.Combine(Application.assetsDirectory, Application.skin, $"{Application.skin}-proteinLogic.xml");

        loadingTitle = "Loading protein logic . . .";
        Debug.Log($"\n{loadingTitle}");
        loadingMessage = "Settings file: " + proteinLogicPath;
        Debug.Log($"{loadingMessage}");
        loadingEvent.Invoke(loadingTitle, loadingMessage);

        if(!ReadProteinLogic())
        {
            Debug.LogError($"\nERROR\n{errorTitle}\n{errorMessage}\n");
            errorEvent.Invoke(errorTitle, errorMessage);
            yield break;
        }

        yield return new WaitForSecondsRealtime(loadingMessageDuration);

        successEvent.Invoke();
    }

    private bool ReadProteinLogic()
    {
        bool result = true;
        List<ProteinLogicBlock> proteinLogicBlocks;

        try
        {
            Type type = typeof(List<ProteinLogicBlock>);
            XmlSerializer serializer;
            FileStream stream;
            serializer = new XmlSerializer(type, new XmlRootAttribute("ProteinLogic"));
            stream = new FileStream(proteinLogicPath, FileMode.Open);
            proteinLogicBlocks = serializer.Deserialize(stream) as List<ProteinLogicBlock>;
            stream.Close();

            Application.settings.allProtienLogic = proteinLogicBlocks.ToDictionary(b => b.pieceName);

            proteinLogicBlocks.RemoveAll(b => b.blockType == ProteinLogicBlock.BlockType.ELSE);
            Application.settings.orderedProteinLogic = proteinLogicBlocks;
        }
        catch (Exception exception)
        {
            Debug.Log("Couldn't read protein logic file.");
            Debug.Log(exception.Message);

            errorTitle = "File not accessible!";
            errorMessage = $"The protein logic file cannot be read. The XML may be incorrectly formatted or malformed.\n\tException: {exception.Message}";
            result = false;
        }

        return result;
    }
}
