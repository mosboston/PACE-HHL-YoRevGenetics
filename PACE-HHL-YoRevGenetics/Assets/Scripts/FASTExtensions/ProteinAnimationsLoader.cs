using FAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

using Application = FAST.Application;

public class ProteinAnimationsLoader : StartupLoader
{
    private string proteinAnimationsPath;

    protected override IEnumerator ExecuteLoad()
    {
        proteinAnimationsPath = Path.Combine(Application.assetsDirectory, Application.skin, $"{Application.skin}-proteinAnimations.xml");

        loadingTitle = "Loading protein animations . . .";
        Debug.Log($"\n{loadingTitle}");
        loadingMessage = "Settings file: " + proteinAnimationsPath;
        Debug.Log($"{loadingMessage}");
        loadingEvent.Invoke(loadingTitle, loadingMessage);

        if (!ReadAnimations())
        {
            Debug.LogError($"\nERROR\n{errorTitle}\n{errorMessage}\n");
            errorEvent.Invoke(errorTitle, errorMessage);
            yield break;
        }

        yield return new WaitForSecondsRealtime(loadingMessageDuration);

        successEvent.Invoke();
    }

    private bool ReadAnimations()
    {
        bool result = true;

        List<ProteinAnimation> animations;

        try
        {

            // Make sure the serializer knows about all the animation command types
            XmlAttributeOverrides attrOverrides = new();
            XmlAttributes attrs = new();
            foreach (Type elementType in AnimationCommand.kCommandTypes)
                attrs.XmlElements.Add(new() { Type = elementType });
            attrOverrides.Add(typeof(ProteinAnimation), "animationCommands", attrs);

            XmlSerializer serializer = new(typeof(List<ProteinAnimation>), attrOverrides, null, new XmlRootAttribute("ProteinAnimations"), null);
            FileStream stream = new (proteinAnimationsPath, FileMode.Open);

            animations = serializer.Deserialize(stream) as List<ProteinAnimation>;
            stream.Close();

            List<string> duplicateNames = animations.GroupBy(a => a.name).Where(a => a.Count() > 1).Select(a => a.Key).ToList();
            if (duplicateNames.Count > 0)
            {
                errorTitle = "There are duplicate animation names!";
                errorMessage += $"\tDuplicate names: {duplicateNames.ToCommaSeparatedString()}";
                result = false;
            }

            animations.ForEach(a => a.Init());
            Application.settings.proteinAnimations = animations.ToDictionary(a => a.name.ToLower());
        }
        catch (Exception exception)
        {
            Debug.Log("Couldn't read protein animation file.");
            Debug.Log(exception.Message);

            errorTitle = "File not accessible!";
            errorMessage = $"The protein animation file cannot be read. The XML may be incorrectly formatted or malformed.\n\tException: {exception.Message}";
            result = false;
        }

        return result;
    }
}
