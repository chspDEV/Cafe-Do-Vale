using GameEventArchitecture.Core.EventSystem.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4.Assets.Resources.Scripts.Systems.Almanaque
{
    public class AlmanaqueCharacterSettings: MonoBehaviour
    {
        public AlmanaqueCharacter myCharacter;

        //PRIMEIRA VISUALIZACAO
        public TextMeshProUGUI characterName;
        public Image characterImage;

        //SEGUNDA VISUALIZACAO (MAIS INFORMACOES)
        public TextMeshProUGUI moreInfo_characterName;
        public TextMeshProUGUI moreInfo_Phrase;
        public TextMeshProUGUI moreInfo_characterDescription;
        public Image moreInfo_characterImage;

        VoidGameEvent OnOpenMoreInfo;

        private void Awake()
        {
            if(myCharacter != null) gameObject.name = "[Almanaque Character] " + myCharacter.characterName;
        }

        public void Setup(BaseAlmanaque almanaque)
        {
            if (myCharacter == null) return;

            if (myCharacter != (AlmanaqueCharacter) almanaque)
            {
                Debug.LogWarning("[AlmanaqueCharacterSettings] Mismatched AlmanaqueCharacter in Setup. Expected: " + myCharacter + ", Received: " + almanaque);
                return;
            }
                

            characterName.text = myCharacter.characterName;
            characterImage.sprite = myCharacter.characterImage;
            OnOpenMoreInfo = myCharacter.OnOpenMoreInfo;
            myCharacter.isUnlocked = true;

            moreInfo_characterName.text = myCharacter.characterName;
            moreInfo_characterImage.sprite = myCharacter.characterImage;
            moreInfo_characterDescription.text = myCharacter.description;
            moreInfo_Phrase.text = myCharacter.phrase;
        }

        public void OpenMoreInfo()
        {
            if(myCharacter != null && myCharacter.isUnlocked == true)
                OnOpenMoreInfo.Broadcast(new());
        }
    }
}
