#nullable enable

namespace Yarn.Unity
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// An implementation of <see cref="IAsyncTypewriter"/> that delivers
    /// characters one at a time, and invokes any <see
    /// cref="IActionMarkupHandler"/>s along the way as needed.
    /// </summary>
    public class BasicTypewriter : IAsyncTypewriter
    {
        /// <summary>
        /// Global multiplier for dialogue typing speed. Values > 1 speed up, < 1 slow down.
        /// </summary>
        public static double GlobalSpeedMultiplier = 1.0;

        /// <summary>
        /// The <see cref="TMP_Text"/> to display the text in.
        /// </summary>
        public TMP_Text? Text { get; set; }

        /// <summary>
        /// A collection of <see cref="IActionMarkupHandler"/> objects that
        /// should be invoked as needed during the typewriter's delivery in <see
        /// cref="RunTypewriter"/>, depending upon the contents of a line.
        /// </summary>
        public IEnumerable<IActionMarkupHandler> ActionMarkupHandlers { get; set; } = Array.Empty<IActionMarkupHandler>();

        /// <summary>
        /// The number of characters per second to deliver.
        /// </summary>
        /// <remarks>If this value is zero, all characters are delivered at
        /// once, subject to any delays added by the markup handlers in <see
        /// cref="ActionMarkupHandlers"/>.</remarks>
        public float CharactersPerSecond { get; set; } = 0f;

        /// <inheritdoc/>
        public async YarnTask RunTypewriter(Markup.MarkupParseResult line, CancellationToken cancellationToken)
        {
            if (Text == null)
            {
                Debug.LogWarning($"Can't show text as typewriter, because {nameof(Text)} was not provided");
            }
            else
            {
                Text.maxVisibleCharacters = 0;
                Text.text = line.Text;

                // Let every markup handler know that display is about to begin
                foreach (var markupHandler in ActionMarkupHandlers)
                {
                    markupHandler.OnLineDisplayBegin(line, Text);
                }

                // Base seconds per character from configured characters-per-second
                double baseSecondsPerCharacter = 0;
                if (CharactersPerSecond > 0)
                {
                    baseSecondsPerCharacter = 1.0 / CharactersPerSecond;
                    // Apply global speed multiplier (divide time to go faster when multiplier > 1)
                    var global = GlobalSpeedMultiplier;
                    if (global <= 0)
                    {
                        global = 0.0001; // avoid divide-by-zero; effectively instant
                    }
                    baseSecondsPerCharacter /= global;
                }

                // Get the count of visible characters from TextMesh to exclude markup characters
                var visibleCharacterCount = Text.GetTextInfo(line.Text).characterCount;

                // Build a per-character speed multiplier map from [speed=...] attributes
                // Defaults to 1.0 (no change). A value > 1 speeds up, < 1 slows down.
                // If an attribute has length 0, ignore it.
                var speedMultipliers = new System.Collections.Generic.Dictionary<int, double>();
                foreach (var attribute in line.Attributes)
                {
                    if (attribute.Name != "speed" || attribute.Length == 0)
                    {
                        continue;
                    }

                    if (attribute.Properties.TryGetValue("speed", out Yarn.Markup.MarkupValue value))
                    {
                        double multiplier = 1.0;
                        switch (value.Type)
                        {
                            case Yarn.Markup.MarkupValueType.Integer:
                                multiplier = value.IntegerValue;
                                break;
                            case Yarn.Markup.MarkupValueType.Float:
                                multiplier = value.FloatValue;
                                break;
                            default:
                                Debug.LogWarning($"Speed property is of type {value.Type}, defaulting to 1.0 (no change).");
                                multiplier = 1.0;
                                break;
                        }

                        // Clamp to a tiny positive value to avoid division by zero or negative speeds
                        if (multiplier <= 0)
                        {
                            multiplier = 0.0001; // effectively instantaneous
                        }

                        // Assign this multiplier to each character index covered by the attribute
                        int start = attribute.Position;
                        int endExclusive = attribute.Position + attribute.Length;
                        for (int idx = start; idx < endExclusive; idx++)
                        {
                            speedMultipliers[idx] = multiplier;
                        }
                    }
                }

                // Helper to get per-character seconds based on multiplier
                double GetSecondsForCharacterIndex(int index)
                {
                    if (baseSecondsPerCharacter <= 0)
                    {
                        return 0; // instant if base is 0 cps
                    }

                    if (speedMultipliers.TryGetValue(index, out var mult))
                    {
                        return baseSecondsPerCharacter / mult;
                    }

                    return baseSecondsPerCharacter;
                }

                // Start with a full time budget so that we immediately show the first character
                double accumulatedDelay = GetSecondsForCharacterIndex(0);

                // Go through each character of the line and letting the
                // processors know about it
                for (int i = 0; i < visibleCharacterCount; i++)
                {
                    // If we don't already have enough accumulated time budget
                    // for a character, wait until we do (or until we're
                    // cancelled)
                    double secondsPerCharacter = GetSecondsForCharacterIndex(i);
                    while (!cancellationToken.IsCancellationRequested
                        && (accumulatedDelay < secondsPerCharacter))
                    {
                        var timeBeforeYield = Time.timeAsDouble;
                        await YarnTask.Yield();
                        var timeAfterYield = Time.timeAsDouble;
                        accumulatedDelay += timeAfterYield - timeBeforeYield;
                    }

                    // Tell every markup handler that it is time to process the
                    // current character
                    foreach (var processor in ActionMarkupHandlers)
                    {
                        await processor
                            .OnCharacterWillAppear(i, line, cancellationToken)
                            .SuppressCancellationThrow();
                    }

                    Text.maxVisibleCharacters += 1;

                    accumulatedDelay -= secondsPerCharacter;
                }

                // We've finished showing every character (or we were
                // cancelled); ensure that everything is now visible.
                Text.maxVisibleCharacters = visibleCharacterCount;
            }

            // Let each markup handler know the line has finished displaying
            foreach (var markupHandler in ActionMarkupHandlers)
            {
                markupHandler.OnLineDisplayComplete();
            }
        }
    }
}
