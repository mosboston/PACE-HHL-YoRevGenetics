using FAST;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Unity.VisualScripting;

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

            // Check for duplicate markerIDs
            List<int> duplicateMarkerIDs = proteinLogicBlocks.GroupBy(b => b.markerID).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateMarkerIDs.Count > 0)
            {
                errorTitle = "There are duplicate marker IDs!";
                foreach (int id in  duplicateMarkerIDs)
                    errorMessage += $"\t{proteinLogicBlocks.Where(b => b.markerID == id).Select(b => b.pieceName).ToCommaSeparatedString()} have ID {id}\n";
                result = false;
            }
            if (proteinLogicBlocks.Any(b => b.markerID == Application.settings.startBlockID))
            {
                errorTitle = "There are duplicate marker IDs!";
                errorMessage += $"\t{proteinLogicBlocks.Where(b => b.markerID == Application.settings.startBlockID).Select(b => b.pieceName).ToCommaSeparatedString()} has the start block ID ({Application.settings.startBlockID})\n";
                result = false;
            }
            if (proteinLogicBlocks.Any(b => b.markerID == Application.settings.endBlockID))
            {
                errorTitle = "There are duplicate marker IDs!";
                errorMessage += $"\t{proteinLogicBlocks.Where(b => b.markerID == Application.settings.endBlockID).Select(b => b.pieceName).ToCommaSeparatedString()} has the end block ID ({Application.settings.endBlockID})\n";
                result = false;
            }

            foreach (ProteinLogicBlock elseBlock in proteinLogicBlocks.FindAll(b => b.blockType == ProteinLogicBlock.BlockType.ELSE))
                elseBlock.action = Application.settings.defaultAction;

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
