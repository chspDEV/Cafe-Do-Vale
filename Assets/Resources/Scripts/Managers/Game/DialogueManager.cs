using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;

// Classe para representar uma linha de di�logo
[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(3, 10)] public string dialogueText;
}

public class DialogueManager : Singleton<DialogueManager>
{
    [System.Serializable]
    public class DialogueEvent : UnityEvent { }

    [Header("Eventos")]
    public DialogueEvent OnDialogueStart;
    public DialogueEvent OnDialogueEnd;

    [Header("Configura��es")]
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

    private bool playerInteracted = false;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        PlayerInteraction.OnPlayerInteraction += CheckPlayerInteraction;
    }

    void CheckPlayerInteraction()
    {
        playerInteracted = true;
        StartCoroutine(nameof(ResetPlayerInteraction));
    }

    IEnumerator ResetPlayerInteraction()
    { 
        yield return new WaitForSeconds(0.25f);
        playerInteracted = false;
    }

    private void Update()
    {
        if (isDialogueActive && playerInteracted)
        {
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
    /// Inicia um novo di�logo
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        if (isDialogueActive) return;

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
        // Ativa a c�mera de di�logo
        CameraManager.Instance.SetDialogueCameraActive(true);

        TimeManager.Instance.Freeze();

        // Pequeno delay para transi��o da c�mera
        yield return new WaitForSeconds(cameraTransitionDelay);

        // Ativa o painel de di�logo
        dialoguePanel.SetActive(true);

        // Dispara evento de di�logo iniciado
        OnDialogueStart.Invoke();

        // Mostra a primeira linha
        DisplayNextLine();
    }

    /// <summary>
    /// Mostra a pr�xima linha do di�logo
    /// </summary>
    public void DisplayNextLine()
    {
        if (currentLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines.Dequeue();
        characterNameText.text = line.characterName;

        // Inicia o efeito de digita��o
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
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
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        DialogueLine currentLine = currentLines.Peek();
        dialogueText.text = currentLine.dialogueText;
        isTyping = false;
        continueIndicator.SetActive(true);
    }

    /// <summary>
    /// Finaliza o di�logo atual
    /// </summary>
    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        // Desativa o painel de di�logo
        dialoguePanel.SetActive(false);

        // Dispara evento de di�logo terminado
        OnDialogueEnd.Invoke();

        // Desativa a c�mera de di�logo
        CameraManager.Instance.SetDialogueCameraActive(false);

        TimeManager.Instance.Unfreeze();

        // Limpa a fila
        currentLines.Clear();
        isDialogueActive = false;
    }
}