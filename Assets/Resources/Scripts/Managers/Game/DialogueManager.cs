using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.InputSystem;


public class DialogueManager : Singleton<DialogueManager>
{
    [Header("Configurações")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float cameraTransitionDelay = 0.5f;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIndicator;

    private Queue<DialogueLine> currentLines = new Queue<DialogueLine>();
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private PlayerInteraction playerInteraction;
    private PlayerMovement playerMovement;

    private DialogueLine currentLine; // Armazena a linha atual

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        playerInteraction = GameAssets.Instance.player.GetComponent<PlayerInteraction>();
        playerMovement = GameAssets.Instance.player.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (playerInteraction != null && playerInteraction.interactionPressed)
        {
            playerInteraction.interactionPressed = false; // Reseta a flag de interação

            if (isTyping)
            {
                CompleteLine();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    /// <summary>
    /// Inicia um novo diálogo
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (isDialogueActive || dialogueData == null || dialogueData.dialogueLines.Count == 0) return;

        playerMovement.Deactive();

        currentLines.Clear();
        foreach (DialogueLine line in dialogueData.dialogueLines)
        {
            currentLines.Enqueue(line);
        }

        isDialogueActive = true;
        StartCoroutine(StartDialogueRoutine());
    }

    private IEnumerator StartDialogueRoutine()
    {
        CameraManager.Instance.SetDialogueCamera(CameraManager.Instance.dialogueCamera, true);
        TimeManager.Instance.Freeze();

        yield return new WaitForSeconds(cameraTransitionDelay);

        dialoguePanel.SetActive(true);
        DeactiveUi.ControlUi(true);

        DisplayNextLine();
    }

    /// <summary>
    /// Mostra a próxima linha do diálogo
    /// </summary>
    public void DisplayNextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = currentLines.Dequeue();
        characterNameText.text = currentLine.characterName;

        typingCoroutine = StartCoroutine(TypeLine(currentLine.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        continueIndicator.SetActive(false);
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        continueIndicator.SetActive(true);
        typingCoroutine = null;
    }

    private void CompleteLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            dialogueText.text = currentLine != null ? currentLine.text : "";
            isTyping = false;
            continueIndicator.SetActive(true);
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }


    /// <summary>
    /// Finaliza o diálogo atual
    /// </summary>
    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        dialoguePanel.SetActive(false);
        DeactiveUi.ControlUi(false);
        CameraManager.Instance.SetDialogueCamera(null,false);
        TimeManager.Instance.Unfreeze();

        playerMovement.Active();

        currentLines.Clear();
        isDialogueActive = false;
        isTyping = false;
        typingCoroutine = null;
        currentLine = null;

        // ✅ Corrige sumiço da missão da UI
        if (QuestManager.Instance.CurrentStep != null)
        {
            QuestUI.Instance.ShowInstruction(QuestManager.Instance.CurrentStep.instructionText);
        }
    }

}
