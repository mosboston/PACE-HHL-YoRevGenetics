using FAST;
using System;
using System.IO;
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

            List<ProteinLogicBlock> defaultBlocks = proteinLogicBlocks.FindAll(b => b.blockType == ProteinLogicBlock.BlockType.DEFAULT);
            if (defaultBlocks.Count < 1)
            {
                errorTitle = "No default action found!";
                errorMessage = "There must be 1 Protein Logic Block with a block type of Default";
                return false;
            }
            if (defaultBlocks.Count > 1)
            {
                errorTitle = "More than one default action found!";
                errorMessage = "There must be only 1 Protein Logic Block with a block type of Default";
                return false;
            }
            if (string.IsNullOrEmpty(defaultBlocks[0].action))
            {
                errorTitle = "No action assigned to default logic block!";
                errorMessage = "The default logic block was found but is missing an action";
                return false;
            }

            Application.settings.defaultAction = defaultBlocks[0].action;
            proteinLogicBlocks.Remove(defaultBlocks[0]);

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
