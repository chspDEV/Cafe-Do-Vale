using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Tcp4
{
    public class UI_Book : MonoBehaviour
    {
        public Button[] buttons;
        public GameObject[] sections;

        void Start()
        {
            foreach(var secao in sections)
                secao.SetActive(false);

            for (int i = 0; i < buttons.Length; i++)
            {
                int sectionIndex = i;

                BookButtonHandler handler = buttons[i].gameObject.AddComponent<BookButtonHandler>();
                handler.Setup(this, sectionIndex);
            }
        }

        public void ShowContent(int index)
        {
            for (int i = 0; i < sections.Length; i++)
                sections[i].SetActive(i == index);
        }

        private class BookButtonHandler : MonoBehaviour, ISelectHandler
        {
            private UI_Book book;
            private int index;

            public void Setup(UI_Book book, int indice)
            {
                this.book = book;
                this.index = indice;
            }

            public void OnSelect(BaseEventData eventData) { book.ShowContent(index); }
        }
    }
}