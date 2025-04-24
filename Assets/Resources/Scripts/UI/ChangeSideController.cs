using UnityEngine;

public class ChangeSideController : MonoBehaviour
{
    //Por padrao é LEFT
    bool isRight = false;
    public GameObject Left;
    public GameObject Right;

    public void Trocar()
    {
        isRight = !isRight;

        Left.SetActive(isRight == false);
        Right.SetActive(isRight == true);
    }




}
