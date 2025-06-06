# Documentation

## Intro

This is a brief summary of the different parts of the Souvenir codebase that you may need when implementing support for a module.

## SouvenirQuestionAttribute

```cs
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SouvenirQuestionAttribute : Attribute
```

Defines a Souvenir question. Add to each new member in the `Souvenir.Question` enum.

### Constructor

```cs
public SouvenirQuestionAttribute(string questionText, string moduleName, AnswerLayout layout, params string[] allAnswers);
```

`questionText` — Include format arguments with the standard curly brace syntax. The `{0}` format argument is reserved for the name of the module.

`moduleName` — Omit the preceding "The" if present.

`layout` — Usually `TwoColumns4Answers` or `ThreeColumns6Answers`. Use `OneColumn4Answers` if the answers will be very long.

`allAnswers` — Specify *every* possible answer. Do not specify any answers if you are using an `AnswerGenerator` or you are obtaining answers from the module in question. See the definition of `Souvenir.AnswerGenerator` for information on answer generators.

### Additional properties

These are additional properties that you may want to set when defining a question.

`bool AddThe` — Set to `true` if the module name begins with "The".

`string[] ExampleExtraFormatArguments` — If you specified extra format arguments, provide example sets of arguments, one after the other. Use the special value `QandA.Ordinal` to specify a random ordinal.

`int ExampleExtraFormatArgumentGroupSize` — The number of extra format arguments. You must specify this if you specified `ExampleExtraFormatArguments`.

`string[] ExampleAnswers` — If *all* of the following apply, specify at least as many example answers as there are displayed answers (4 or 6 depending on `layout`):

- you did not specify `allAnswers`
- you are not using an `AnswerGenerator`
- you are not using sprite answers.

`AnswerType Type` — Used to specify a different answer type: grids, sprites, font, or audio. See the definition of `Souvenir.AnswerType` for information on what to specify.

`bool UsesQuestionSprite` — A "question sprite" is an image that shows up with the question. See the *Coloured Cubes* question for an example of this.

`string SpriteFieldName` — If you are using `AnswerType.Sprites` and these sprites are included in the Souvenir project, specify the name of the field holding the `Sprite[]`. See the *Question Mark* question for an example of this.

`int FontSize` and `float CharacterSize` — These correspond to their respective `TextMesh` properties. Useful for increasing the resolution of complex fonts, for example in *Snowflakes* and *Sugar Skulls*.

`bool IsEntireQuestionSprite` — Use this if the theme of the module in question warrants it. Examples include *❖* and *Technical Keypad*.

Note: The next three apply to `AnswerType.Audio`.

`string AudioFieldName` — If the audio is included in the Souvenir project, specify the name of the field holding the `AudioClip[]` as in the *Listening* question. If possible, prefer to use a `ForeignAudioID` instead.

`string ForeignAudioID` — The mod ID where the `AudioClip`s come from. Note that this is *not* the module ID. This is the ID specified in `modInfo.json` and used as the name for a mod's `.dll` file. It is specified in `Assets/Editor/Resources/ModConfig.asset`. This only works if all of the `AudioClip`s can be obtained via reflection. If not, use an `AudioFieldName` instead.

`float AudioSizeMultiplier` — Visually scales waveforms for audio clips. A value of `1f` would be appropriate for constantly peaking audio. Increase this to achieve better visuals. The default value is `2f`.

Note: If you are unsure about the next two, do not specify them as a translator can do this at a later point in time.

`bool TranslateAnswers` — Should the answers be translatable?

`bool[] TranslateFormatArgs` — Should each format argument be translatable?

## Reflection helpers

Souvenir uses reflection to access information on other modules on the bomb. There are several types and methods in order to help with this.

### Getting members

#### Fields

```cs
private FieldInfo<T> GetField<T>(object target, string name, bool isPublic = false);

private IntFieldInfo GetIntField(object target, string name, bool isPublic = false);

private ArrayFieldInfo<T> GetArrayField<T>(object target, string name, bool isPublic = false);

private ListFieldInfo<T> GetListField<T>(object target, string name, bool isPublic = false);
```

Get a instance field called `name` from `target`.

```cs
private FieldInfo<T> GetStaticField<T>(Type targetType, string name, bool isPublic = false);
```

Get a static field from `targetType`.

---

In `GetField` and `GetStaticField`, `T` is the type stored in the field.

In `GetArrayField` and `GetListField`, `T` is the type stored in the collection in the field.

Be sure to use the most applicable method because they make validating data easier (see **Getting Values** below).

#### Properties

```cs
private PropertyInfo<T> GetProperty<T>(object target, string name, bool isPublic = false);
```

Get an instance property called `name` of type `T` from `target`.

```cs
private PropertyInfo<T> GetStaticProperty<T>(Type targetType, string name, bool isPublic = false);
```

Get a static property from `targetType`.

#### Methods

```cs
private MethodInfo<T> GetMethod<T>(object target, string name, int numParameters, bool isPublic = false);
```

Get an instance method with return type `T` called `name` from `target`.

```cs
private MethodInfo<object> GetMethod(object target, string name, int numParameters, bool isPublic = false);
```

Get an instance method with return type `void` called `name` from `target`.

```cs
private MethodInfo<T> GetStaticMethod<T>(Type targetType, string name, int numParameters, bool isPublic = false);

private MethodInfo<object> GetStaticMethod(Type targetType, string name, int numParameters, bool isPublic = false);
```

Get a static method from `targetType`.

---

### Getting values

All of the `FieldInfo<T>` and `PropertyInfo<T>` types from the above section derive from `InfoBase<T>`. They have several `Get` method overloads for getting the data stored in the field or property, and validating that data. Each `Get` overload has a corresponding `GetFrom` overload, which requires a target. This means you do not need to access the same field from several instances of the same type.

```cs
public T Get(Func<T, string> validator = null, bool nullAllowed = false);

public T GetFrom(object obj, Func<T, string> validator = null, bool nullAllowed = false);
```

`validator` — should return `null` when the data is valid, otherwise return a `string` explaining why it is invalid. Example:

```cs
var fldMyValue = GetField<string>(comp, "_myValue");
var myValue = fldMyValue.Get(validator: x => x.length > 5 ? "expected length <= 5" : null);
```

You should *always* use validators when applicable, since they tend to provide much nicer error messages than some error later down the line would.

You can pass other arguments to help further validate data, such as `nullAllowed`. The other overloads have more:

#### IntFieldInfo

```cs
public int Get(int? min = null, int? max = null);
```

`min` and `max` are both inclusive.

#### ArrayFieldInfo&lt;T&gt;, ListFieldInfo&lt;T&gt;

```cs
// TCollection is T[] or List<T> depending on type
// TElement is T
public TCollection Get(int expectedLength, bool nullArrayAllowed = false, bool nullContentAllowed = false, Func<TElement, string> validator = null);

public TCollection Get(int minLength, int? maxLength = null, bool nullArrayAllowed = false, bool nullContentAllowed = false, Func<TElement, string> validator = null);
```

Here, each individual element is checked against `validator`. You can still use the standard overload above to run a validator against the collection object itself, but this will always reject a collection with any `null` elements.

#### PropertyInfo&lt;T&gt;

```cs
public T Get(object[] index, Func<T, string> validator = null, bool nullAllowed = false)
```

`index` — the index to specify for indexed properties.

### Setting values

You can also set fields and properties to new values using `.Set()` or `.SetTo()`. This should be used very sparingly as it means you’re making a change to the target module instead of just reading out its info. Only do this if you know what you’re doing.

### Invoking methods

#### MethodInfo&lt;T&gt;

```cs
public T Invoke(params object[] arguments);
public T InvokeOn(object target, params object[] arguments);
```

## Setting questions

### Important

Before making any questions, you *must* wait for the module to be solved. You can use `yield return WaitForSolve;` to do this.
For modules where this is not possible (such as bosses), either specify the solve order manually or give a differently formatted module name.
For example, *Forget Me Not* will use a phrase such as “the Forget Me Not which displayed a 5 in the first stage” to identify the specific module.

### No questions

If, for any reason, Souvenir cannot generate any questions for a given module, you *must* call `legitimatelyNoQuestion` and provide a reason:

```cs
// Langton's Anteater
legitimatelyNoQuestion(module, "the module generated 25 cells of the same colour.");
yield break;
```

### Making questions

There are several `makeQuestion` and `makeSpriteQuestion` overloads, which are for different question and answer types
(there exist even more overloads for niche use cases):

```cs
// Default
private QandA makeQuestion(Question question, ModuleData data, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArgs = null, string[] correctAnswers = null, string[] preferredWrongAnswers = null, string[] allAnswers = null, float questionSpriteRotation = 0);

// Dynamic font answers
private QandA makeQuestion(Question question, ModuleData data, Font font, Texture fontTexture, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArgs = null, string[] correctAnswers = null, string[] preferredWrongAnswers = null, string[] allAnswers = null, float questionSpriteRotation = 0);

// Sprite answers
private QandA makeQuestion(Question question, ModuleData data, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArgs = null, Sprite[] correctAnswers = null, Sprite[] preferredWrongAnswers = null, Sprite[] allAnswers = null, float questionSpriteRotation = 0);

// Grid answers
private QandA makeQuestion(Question question, ModuleData data, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArgs = null, Coord[] correctAnswers = null, Coord[] preferredWrongAnswers = null, float questionSpriteRotation = 0);

// Audio answers
private QandA makeQuestion(Question question, ModuleData data, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArgs = null, AudioClip[] correctAnswers = null, AudioClip[] preferredWrongAnswers = null, AudioClip[] allAnswers = null, float questionSpriteRotation = 0);

// Sprite question, text answers
private QandA makeSpriteQuestion(Sprite questionSprite, Question question, ModuleData data, string formattedModuleName = null, string[] formatArgs = null, string[] correctAnswers = null, string[] preferredWrongAnswers = null, string[] allAnswers = null);

// Sprite question, sprite answers
private QandA makeSpriteQuestion(Sprite questionSprite, Question question, ModuleData data, string formattedModuleName = null, string[] formatArgs = null, Sprite[] correctAnswers = null, Sprite[] preferredWrongAnswers = null, Sprite[] allAnswers = null);

// Sprite question, audio answers
private QandA makeSpriteQuestion(Sprite questionSprite, Question question, ModuleData data, string formattedModuleName = null, string[] formatArgs = null, AudioClip[] correctAnswers = null, AudioClip[] preferredWrongAnswers = null, AudioClip[] allAnswers = null);
```

In every case, the meaning of each parameter is equivalent:

`question` — The question being asked.

`data` — The object passed in to the module handler.

`questionSprite` — For non-sprite questions, this sprite appears to the right of the question and usually highlights a location in a grid (example: *cRule*). For sprite questions, this is the sprite that represents the question (example: *Technical Keypad*).

`formattedModuleName` — Used for boss modules to specify a uniquely identifying factor for the module being asked about (example: *Forget Me Not*). Leave unset most of the time.

`formatArgs` — Extra format arguments used in the question.

`correctAnswers` — *Every* possible correct answer. Souvenir will display only one of these.

`preferredWrongAnswers` — Souvenir will display these as possible answers before choosing randomly from all available answers.

`allAnswers` — If you did not specify all answers in the `SouvenirQuestionAttribute` constructor and you are not using an `AnswerGenerator`, you *must* specify all possible answers here.

`questionSpriteRotation` — clockwise, measured in degrees.

### Adding questions

To add a question batch, call `addQuestions`. You *must not* add more than one question batch per module—make all of the questions first and add them in one go at the end.

```cs
private void addQuestions(ModuleData module, IEnumerable<QandA> questions);

private void addQuestions(ModuleData module, params QandA[] questions);
```

If you are only adding one question, you can use one of the `addQuestion` overloads instead:

```cs
private void addQuestion(ModuleData module, Question question, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArguments = null, string[] correctAnswers = null, string[] preferredWrongAnswers = null, string[] allAnswers = null, float questionSpriteRotation = 0);

private void addQuestion(ModuleData module, Question question, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArguments = null, Sprite[] correctAnswers = null, Sprite[] allAnswers = null, Sprite[] preferredWrongAnswers = null, float questionSpriteRotation = 0);

private void addQuestion(ModuleData module, Question question, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArguments = null, Coord[] correctAnswers = null, Coord[] preferredWrongAnswers = null, float questionSpriteRotation = 0);

private void addQuestion(ModuleData module, Question question, Sprite questionSprite = null, string formattedModuleName = null, string[] formatArguments = null, AudioClip[] correctAnswers = null, AudioClip[] allAnswers = null, AudioClip[] preferredWrongAnswers = null, float questionSpriteRotation = 0);
```

### Using sprites

Fundamentally, there are three ways to create a question that uses sprites as answers:

* Import the sprites from the target module. As an example, *Module Maze* uses this to show module icons as answers. The target module often has a field of type `Sprite[]` that you can just read out. In addition to passing the correct answer sprite(s) to `correctAnswers` as usual, make sure to also pass the full list of all possible sprites to `allAnswers`.
* Generate the sprites on the fly at runtime. If the module is just an n×m rectilinear grid, you can just use the `makeQuestion` overloads that take `Coord` objects, which will automatically be converted to sprites that represent a grid with one cell highlighted. If it’s a little more involved, feel free to look at how `Sprites.GenerateGridSprite` generates the grid sprites and use it as inspiration to make your own sprite generator.
* Include the sprites in the Souvenir mod itself, as a field of type `Sprite[]`. For example, `ArithmelogicSprites` contains the sprites for one of the Arithmelogic questions. If at all possible, this should only be used if the other two options are not available, as it means the sprites are shipped with Souvenir, increasing the size of the mod. If you do decide for this option, make sure to:
	* Declare a field of type `Sprite[]` and assign all the sprites you need using the Unity editor.
	* In the question attribute, use `Type = AnswerType.Sprites` and `SpriteFieldName = "ArithmelogicSprites"` (except of course put the new field’s name).
	* Now you can simply use these sprites in your call to `makeQuestion`.

