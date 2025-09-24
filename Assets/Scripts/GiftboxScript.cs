using UnityEngine;

public class GiftboxScript : MonoBehaviour
{
    public void OnOpenned()
    {
        GetComponent<Animator>().Play("Idle");
        UIManager.Instance.StartCoroutine(ShowGachaResult());
    }
}
