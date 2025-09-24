using UnityEngine;
using UnityEngine.UI; // Button, Image 사용
using TMPro; // TextMeshProUGUI 사용
using DG.Tweening; // DOTween 사용
using System.Collections; // Coroutine 사용

public class UIManager : MonoBehaviour
{
    // 싱글톤
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("UIManager가 씬에 배치되지 않았습니다.");
            return instance;
        }
    }

    [Header("UI References")]
    public Canvas canvas;
    public Button confirmButton;
    public Button gachaButton;
    private float gachaButton_originY; // 버튼 위치
    public GameObject giftBox;     // 선물 상자
    public GameObject rewardPanel; // 결과물 이미지와 텍스트를 포함하는 부모 패널
    private float rewardPanel_originY;
    public Image rewardImage;
    public TextMeshProUGUI rewardText;

    [Header("Gacha Items")]
    public Sprite[] itemSprites; // 뽑기 결과로 나올 아이템 이미지들
    public string[] itemNames;   // 아이템 이름들

    [Header("Effect References")]
    public GameObject effectPrefab; // 빠밤! 효과에 사용할 이펙트 프리팹

    [Header("Animation Settings")]
    public float revealDuration = 0.5f; // 결과물이 나타나는 시간
    public float scalePunchStrength = 1.2f; // 빠밤! 스케일 애니메이션 강도
    public float effectDelay = 0.1f; // 결과물과 이펙트 등장 시간 차이

    // 애니메이터 제어를 위해 필요한 Animator 컴포넌트
    private Animator giftBoxAnimator;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 결과 패널의 Animator 컴포넌트 가져오기
        giftBoxAnimator = giftBox.GetComponent<Animator>();
        if (giftBoxAnimator == null)
        {
            Debug.LogError("GiftBox에 Animator 컴포넌트가 없습니다!");
        }

        // 결과 패널과 버튼 초기 위치 저장
        rewardPanel_originY = rewardPanel.GetComponent<RectTransform>().localPosition.y;
        gachaButton_originY = gachaButton.GetComponent<RectTransform>().localPosition.y;

        // 버튼 이벤트 등록
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        gachaButton.onClick.AddListener(OnGachaButtonClick);

        // 결과 패널 위치 초기화
        rewardPanel.GetComponent<RectTransform>().localPosition = new Vector3(0, 2400, 0);
        rewardPanel.SetActive(false);
    }

    // ConfirmButton 클릭 시 호출되는 콜백 메소드
    void OnConfirmButtonClick()
    {
        // ConfirmButton이 아래로 사라짐
        confirmButton.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, -1200, 0), 0.5f)
            .SetEase(Ease.OutBack);

        // RewardPanel이 위로 사라짐
        rewardPanel.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, 2400, 0), 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => { giftBoxAnimator.Play("Creating"); });

        // GachaButton이 gachButton_originY 위치에 나타남
        gachaButton.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, gachaButton_originY, 0), 0.5f)
            .SetDelay(0.5f) // ConfirmButton이 내려가고 난 뒤에 DOLocalMove를 실행하기 위함
            .SetEase(Ease.OutBack) // OutBack 형식으로 애니메이션
            .OnComplete(() => { gachaButton.interactable = true; } // 애니메이션이 완료되면 GachaButton 기능 활성화
            );
    }

    // GachaButton 클릭 시 호출되는 콜백 메소드
    void OnGachaButtonClick()
    {
        Debug.Log("뽑기 버튼 클릭!");
        gachaButton.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, -1200, 0), 0.5f)
            .SetEase(Ease.InBack);

        gachaButton.interactable = false; // 뽑기 중에는 버튼 비활성화
        rewardPanel.SetActive(false); // 이전 결과 숨기기

        // GiftBox 흔들리는 애니메이션 실행
        giftBoxAnimator.SetTrigger("Openned");
    }

    // 뽑기 시도 후 GiftBox 애니메이션 완료 시 호출되는 콜백 메소드
    public void ShowGachaReward()
    {
        if (effectPrefab != null)
        {
            // 파티클 오브젝트 생성
            GameObject effectInstance = Instantiate(effectPrefab, giftBox.transform);
            effectInstance.GetComponent<RectTransform>().SetParent(canvas.transform, false);

            // 파티클 효과 시작
            ParticleSystem effectParticleSystem = effectInstance.GetComponent<ParticleSystem>();
            if (effectParticleSystem != null)
            {
                effectParticleSystem.Play();
            }

            // 2초 후 파티클 삭제
            Destroy(effectInstance, 2f); // 2초 후 이펙트 삭제
        }

        // 뽑기 로직 실행
        StartCoroutine(ShowGacharewardEnumerator());
    }

    // 실제 뽑기 로직
    IEnumerator ShowGacharewardEnumerator()
    {
        // 랜덤 아이템 선택
        int randomIndex = Random.Range(0, itemSprites.Length);
        Sprite selectedSprite = itemSprites[randomIndex];
        string selectedName = itemNames[randomIndex];

        rewardImage.sprite = selectedSprite;
        rewardText.text = selectedName;

        // 결과 패널 활성화
        rewardPanel.SetActive(true);

        // RewardPanel이 위에서 등장
        rewardPanel.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, rewardPanel_originY, 0), 0.5f)
            .SetEase(Ease.InBack);

        // 결과 패널 아이템이 나타나는 동안 대기
        // RewardPanel 이 자리를 잡고 아이템이 나타나기까지 걸리는 시간동안 대기
        yield return new WaitForSeconds(revealDuration); // Animator 애니메이션 길이에 맞춰 대기

        // 결과 패널 이미지와 텍스트가 서서히 나타나고 커지는 효과
        rewardImage.color = new Color(1, 1, 1, 0); // 투명하게 시작
        rewardText.color = new Color(1, 1, 1, 0);

        // DOTween 시퀀스 생성
        Sequence revealSequence = DOTween.Sequence();

        // 이미지와 텍스트 서서히 나타남
        revealSequence.Append(rewardImage.DOFade(1, revealDuration * 0.5f));
        revealSequence.Join(rewardText.DOFade(1, revealDuration * 0.5f));

        // DOPunchScale 효과
        // 페이드 인효과 이후에 한번 더 펀치 효과가 나타남
        revealSequence.Append(rewardImage.transform.DOPunchScale(Vector3.one * scalePunchStrength, revealDuration * 0.5f, 5, 0.5f));
        revealSequence.Join(rewardText.transform.DOPunchScale(Vector3.one * scalePunchStrength, revealDuration * 0.5f, 5, 0.5f));

        // 시퀀스 재생 대기
        yield return revealSequence.WaitForCompletion();

        // 뽑기 종료 후 버튼 활성화
        confirmButton.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, gachaButton_originY, 0), 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { gachaButton.interactable = true; }
            );
    }
}