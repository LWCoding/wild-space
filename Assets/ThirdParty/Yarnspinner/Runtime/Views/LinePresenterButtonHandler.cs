/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Markup;
using Yarn.Unity.Attributes;

#if USE_TMP
using TMPro;
#else
using TMP_Text = Yarn.Unity.TMPShim;
#endif

#nullable enable

namespace Yarn.Unity
{
    public class LinePresenterButtonHandler : ActionMarkupHandler
    {
        [MustNotBeNull, SerializeField] Button? continueButton;

        [MustNotBeNullWhen(nameof(continueButton), "A " + nameof(DialogueRunner) + " must be provided for the continue button to work.")]
        [SerializeField] DialogueRunner? dialogueRunner;

        private bool isLineFullyRendered = false;

        void Awake()
        {
            if (continueButton == null)
            {
                Debug.LogWarning($"The {nameof(continueButton)} is null, is it not connected in the inspector?", this);
                return;
            }
            continueButton.interactable = false;
            continueButton.enabled = false;
            
            // Subscribe to the typewriter finished event
            LinePresenter.OnTypewriterFinished += OnTypewriterFinished;
        }

        void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            LinePresenter.OnTypewriterFinished -= OnTypewriterFinished;
        }

        public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
        {
            if (continueButton == null)
            {
                Debug.LogWarning($"The {nameof(continueButton)} is null, is it not connected in the inspector?", this);
                return;
            }
            
            // Reset state for new line
            isLineFullyRendered = false;
            
            // enable the button
            continueButton.interactable = true;
            continueButton.enabled = true;

            continueButton.onClick.AddListener(() =>
            {
                if (dialogueRunner == null)
                {
                    Debug.LogWarning($"Continue button was clicked, but {nameof(dialogueRunner)} is null!", this);
                    return;
                }

                // If line is already fully rendered, advance immediately; otherwise hurry it up
                if (isLineFullyRendered)
                {
                    dialogueRunner.RequestNextLine();
                }
                else
                {
                    dialogueRunner.RequestHurryUpLine();
                }
            });
        }

        public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {
            return;
        }

        public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {
            return YarnTask.CompletedTask;
        }

        public override void OnLineDisplayComplete()
        {
            return;
        }

        public override void OnLineWillDismiss()
        {
            if (continueButton == null)
            {
                return;
            }
            // disable interaction
            continueButton.onClick.RemoveAllListeners();
            continueButton.interactable = false;
            continueButton.enabled = false;
        }

        /// <summary>
        /// Called when the typewriter animation finishes rendering the line.
        /// We only mark that the line is fully rendered; advancing requires a
        /// subsequent click.
        /// </summary>
        private void OnTypewriterFinished()
        {
            isLineFullyRendered = true;
        }
    }
}
