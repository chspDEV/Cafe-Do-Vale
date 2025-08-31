// Scripts/AlmanaqueUI.cs

using Tcp4; // Namespace do seu BaseEventListener
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Assets.Resources.Scripts.Systems.Almanaque;

public class AlmanaqueUI : MonoBehaviour
{
    public List<AlmanaqueCharacterSettings> charactersList;
    public GameObject containerCharacters;
    public AlmanaqueEventListener listener; // Seu listener concreto

    private void Start()
    {
        // 1. Encontra todos os componentes dos personagens
        var characters = containerCharacters.GetComponentsInChildren<AlmanaqueCharacterSettings>().ToList();
        charactersList = new(characters);

        // 2. Garante que o listener não é nulo antes de continuar
        if (listener == null)
        {
            Debug.LogError("O AlmanaqueEventListener não foi atribuído no Inspector!", this);
            return;
        }

        // 3. Loop para registrar o método 'Setup' de cada personagem no evento
        foreach (var character in charactersList)
        {
            // Adiciona o método 'Setup' do personagem atual como um ouvinte
            // Acessamos a propriedade 'UnityEventResponse' que criamos no Passo 1
            listener.UnityEventResponse.AddListener(character.Setup);
        }
    }
}