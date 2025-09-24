using UnityEngine;

public class GiftboxScript : MonoBehaviour
{
    // 애니메이션 이벤트에 등록할 콜백 메소드
    public void OnOpenned()
    {
        UIManager.Instance.ShowGachaReward();
    }
}
