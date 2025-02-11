﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Souvenir;
using UnityEngine;

public partial class SouvenirModule
{
    private IEnumerator<YieldInstruction> ProcessIceCream(ModuleData module)
    {
        var comp = GetComponent(module, "IceCreamModule");
        var fldCurrentStage = GetIntField(comp, "CurrentStage");
        var fldCustomers = GetArrayField<int>(comp, "CustomerNamesSolution");
        var fldSolution = GetArrayField<int>(comp, "Solution");
        var fldFlavourOptions = GetArrayField<int[]>(comp, "FlavorOptions");

        while (!_isActivated)
            yield return new WaitForSeconds(.1f);

        var flavourNames = Question.IceCreamFlavour.GetAnswers();
        var customerNames = Question.IceCreamCustomer.GetAnswers();

        var flavours = new int[3][];
        var solution = new int[3];
        var customers = new int[3];

        for (var i = 0; i < 3; i++)
        {
            while (fldCurrentStage.Get() == i)
                yield return new WaitForSeconds(.1f);
            if (fldCurrentStage.Get() < i)
                throw new AbandonModuleException($"The stage number went down from {i} to {fldCurrentStage.Get()}.");

            var options = fldFlavourOptions.Get(expectedLength: 3, validator: x => x.Length != 5 ? "expected length 5" : x.Any(y => y < 0 || y >= flavourNames.Length) ? $"expected range 0–{flavourNames.Length - 1}" : null);
            var sol = fldSolution.Get(ar => ar.Any(x => x < 0 || x >= flavourNames.Length) ? $"expected range 0–{flavourNames.Length - 1}" : null);
            var cus = fldCustomers.Get(ar => ar.Any(x => x < 0 || x >= customerNames.Length) ? $"expected range 0–{customerNames.Length - 1}" : null);

            flavours[i] = options[i].ToArray();
            solution[i] = flavours[i][sol[i]];
            customers[i] = cus[i];
        }
        var qs = new List<QandA>();
        yield return WaitForSolve;

        for (var i = 0; i < 3; i++)
        {
            qs.Add(makeQuestion(Question.IceCreamFlavour, module, formatArgs: new[] { "was on offer, but not sold,", Ordinal(i + 1) }, correctAnswers: flavours[i].Where(ix => ix != solution[i]).Select(ix => flavourNames[ix]).ToArray()));
            qs.Add(makeQuestion(Question.IceCreamFlavour, module, formatArgs: new[] { "was not on offer", Ordinal(i + 1) }, correctAnswers: flavourNames.Where((f, ix) => !flavours[i].Contains(ix)).ToArray()));
            if (i != 2)
                qs.Add(makeQuestion(Question.IceCreamCustomer, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { customerNames[customers[i]] }, preferredWrongAnswers: customers.Select(ix => customerNames[ix]).ToArray()));
        }

        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessIdentificationCrisis(ModuleData module)
    {
        var comp = GetComponent(module, "identificationCrisis");
        yield return WaitForSolve;

        var shapes = GetArrayField<int>(comp, "shapesUsed").Get();
        var datasets = GetArrayField<int>(comp, "datasetsUsed").Get();
        var shapeNames = GetStaticField<string[]>(comp.GetType(), "shapeNames").Get();
        var datasetNames = new[] { "Morse Identification", "Boozleglyph Identification", "Plant Identification", "Pickup Identification", "Emotiguy Identification", "Ars Goetia Identification", "Mii Identification", "Customer identification", "Spongebob Birthday Identification", "VTuber Identification" };
        var qs = new List<QandA>();
        for (int i = 0; i < 3; i++)
        {
            qs.Add(makeQuestion(Question.IdentificationCrisisShape, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { shapeNames[shapes[i]] }));
            qs.Add(makeQuestion(Question.IdentificationCrisisDataset, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { datasetNames[datasets[i]] }));
        }
        addQuestions(module, qs);
    }

    private IEnumerator<YieldInstruction> ProcessIdentityParade(ModuleData module)
    {
        var comp = GetComponent(module, "identityParadeScript");

        yield return WaitForSolve;

        foreach (var btnFieldName in new[] { "hairLeft", "hairRight", "buildLeft", "buildRight", "attireLeft", "attireRight", "suspectLeft", "suspectRight", "convictBut" })
        {
            var btn = GetField<KMSelectable>(comp, btnFieldName, isPublic: true).Get();
            btn.OnInteract = delegate
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
                btn.AddInteractionPunch(0.5f);
                return false;
            };
        }

        var textMeshes = new[] { "hairText", "buildText", "attireText", "suspectText" }.Select(fldName => GetField<TextMesh>(comp, fldName, isPublic: true).Get()).ToArray();
        textMeshes[0].text = "Identity";
        textMeshes[1].text = "Parade";
        textMeshes[2].text = "has been";
        textMeshes[3].text = "solved";

        var hairs = GetListField<string>(comp, "hairEntries").Get(expectedLength: 3);
        var builds = GetListField<string>(comp, "buildEntries").Get(expectedLength: 3);
        var attires = GetListField<string>(comp, "attireEntries").Get(expectedLength: 3);

        var validHairs = new[] { "Black", "Blonde", "Brown", "Grey", "Red", "White" };
        var validBuilds = new[] { "Fat", "Hunched", "Muscular", "Short", "Slim", "Tall" };
        var validAttires = new[] { "Blazer", "Hoodie", "Jumper", "Suit", "T-shirt", "Tank top" };

        addQuestions(module,
            makeQuestion(Question.IdentityParadeHairColors, module, formatArgs: new[] { "was" }, correctAnswers: hairs.ToArray()),
            makeQuestion(Question.IdentityParadeHairColors, module, formatArgs: new[] { "was not" }, correctAnswers: validHairs.Except(hairs).ToArray()),
            makeQuestion(Question.IdentityParadeBuilds, module, formatArgs: new[] { "was" }, correctAnswers: builds.ToArray()),
            makeQuestion(Question.IdentityParadeBuilds, module, formatArgs: new[] { "was not" }, correctAnswers: validBuilds.Except(builds).ToArray()),
            makeQuestion(Question.IdentityParadeAttires, module, formatArgs: new[] { "was" }, correctAnswers: attires.ToArray()),
            makeQuestion(Question.IdentityParadeAttires, module, formatArgs: new[] { "was not" }, correctAnswers: validAttires.Except(attires).ToArray()));
    }

    private IEnumerator<YieldInstruction> ProcessImpostor(ModuleData module)
    {
        var comp = GetComponent(module, "impostorScript");

        yield return WaitForSolve;

        var possibleModuleNames = GetArrayField<GameObject>(comp, "Prefabs", isPublic: true).Get().Select(pref => pref.name).ToArray();
        var chosenModIndex = GetIntField(comp, "chosenMod").Get(min: 0, max: possibleModuleNames.Length - 1);
        addQuestion(module, Question.ImpostorDisguise, correctAnswers: new[] { possibleModuleNames[chosenModIndex] }, preferredWrongAnswers: possibleModuleNames);
    }

    private IEnumerator<YieldInstruction> ProcessIndigoCipher(ModuleData module)
    {
        return processColoredCiphers(module, "indigoCipher", Question.IndigoCipherScreen);
    }

    private IEnumerator<YieldInstruction> ProcessInfiniteLoop(ModuleData module)
    {
        var comp = GetComponent(module, "InfiniteLoop");
        yield return WaitForSolve;

        var selectedWord = GetField<string>(comp, "SelectedWord").Get();
        addQuestion(module, Question.InfiniteLoopSelectedWord, correctAnswers: new[] { selectedWord });
    }

    private IEnumerator<YieldInstruction> ProcessIngredients(ModuleData module)
    {
        var comp = GetComponent(module, "IngredientsScript");
        var initialIngredients = GetField<Array>(comp, "InitialIngredientsList").Get().Cast<object>().Select(ev => ev.ToString()).ToArray();
        yield return WaitForSolve;

        var unusedIngredients = GetField<IList>(comp, "CurrentIngredientsList").Get().Cast<object>().Select(ev => ev.ToString()).ToArray();
        var usedIngredients = initialIngredients.Except(unusedIngredients).ToArray();

        addQuestions(module,
            makeQuestion(Question.IngredientsIngredients, module, correctAnswers: usedIngredients, preferredWrongAnswers: unusedIngredients),
            makeQuestion(Question.IngredientsNonIngredients, module, correctAnswers: unusedIngredients, preferredWrongAnswers: usedIngredients));
    }

    private IEnumerator<YieldInstruction> ProcessInnerConnections(ModuleData module)
    {
        var comp = GetComponent(module, "InnerConnectionsScript");
        var morseNumber = GetField<int>(comp, "morseNumber").Get();
        var rndLEDColour = GetField<int>(comp, "rndLEDColour").Get();
        yield return WaitForSolve;

        var colourList = new[] { "Black", "Blue", "Red", "White", "Yellow" };

        addQuestions(module,
            makeQuestion(Question.InnerConnectionsLED, module, correctAnswers: new[] { colourList[rndLEDColour] }),
            makeQuestion(Question.InnerConnectionsMorse, module, correctAnswers: new[] { morseNumber.ToString() }));
    }

    private IEnumerator<YieldInstruction> ProcessInterpunct(ModuleData module)
    {
        var comp = GetComponent(module, "InterpunctScript");
        var fldDisplay = GetField<string>(comp, "displaySymbol");
        var fldStage = GetIntField(comp, "stage");

        var currentStage = 0;
        var texts = new string[3];
        var hasStruck = false;
        module.Module.OnStrike += delegate () { hasStruck = true; return false; };

        while (module.Unsolved)
        {
            yield return null;
            var nextStage = fldStage.Get(min: 1, max: 3);   // stage numbers are 1–3, not 0–2
            if (currentStage != nextStage || hasStruck)
            {
                currentStage = nextStage;
                texts[currentStage - 1] = fldDisplay.Get();
                hasStruck = false;
            }
        }

        addQuestions(module, Enumerable.Range(0, 3).Select(i =>
            makeQuestion(Question.InterpunctDisplay, module, formatArgs: new[] { Ordinal(i + 1) }, correctAnswers: new[] { texts[i] })));
    }

    private IEnumerator<YieldInstruction> ProcessIPA(ModuleData module)
    {
        var comp = GetComponent(module, "ipa");
        yield return WaitForSolve;

        var sounds = GetArrayField<AudioClip>(comp, "sounds", true).Get(expectedLength: 71);
        var cap = GetField<int>(comp, "cap").Get(i => i != 44 && i != 71 ? $"Unknown cap value {i} (expected 44 or 71)" : null);

        var soundIx = GetIntField(comp, "soundPresent").Get(0, sounds.Length - 1);
        addQuestions(module, makeQuestion(Question.IpaSound, module,
            correctAnswers: new[] { sounds[soundIx] },
            allAnswers: sounds.Take(cap).ToArray()));
    }

    private IEnumerator<YieldInstruction> ProcessiPhone(ModuleData module)
    {
        var comp = GetComponent(module, "iPhoneScript");
        var digits = GetListField<string>(comp, "pinDigits", isPublic: true).Get(expectedLength: 4);

        yield return WaitForSolve;

        addQuestions(module,
            makeQuestion(Question.iPhoneDigits, module, formatArgs: new[] { Ordinal(1) }, correctAnswers: new[] { digits[0] }, preferredWrongAnswers: new[] { digits[1], digits[2], digits[3] }),
            makeQuestion(Question.iPhoneDigits, module, formatArgs: new[] { Ordinal(2) }, correctAnswers: new[] { digits[1] }, preferredWrongAnswers: new[] { digits[0], digits[2], digits[3] }),
            makeQuestion(Question.iPhoneDigits, module, formatArgs: new[] { Ordinal(3) }, correctAnswers: new[] { digits[2] }, preferredWrongAnswers: new[] { digits[1], digits[0], digits[3] }),
            makeQuestion(Question.iPhoneDigits, module, formatArgs: new[] { Ordinal(4) }, correctAnswers: new[] { digits[3] }, preferredWrongAnswers: new[] { digits[1], digits[2], digits[0] }));
    }
}
