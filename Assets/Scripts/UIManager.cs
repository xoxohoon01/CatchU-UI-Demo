using UnityEngine;
using UnityEngine.UI; // Button, Image 사용
using TMPro; // TextMeshProUGUI 사용
using DG.Tweening; // DOTween 사용
using System.Collections; // Coroutine 사용

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button gachaButton;
    public GameObject giftBox;     // 선물 상자
    public GameObject resultPanel; // 결과물 이미지와 텍스트를 포함하는 부모 패널
    public Image resultImage;
    public TextMeshProUGUI resultText;

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
    private Animator resultPanelAnimator;

    void Start()
    {
        // DOTween 초기화 (만약 Start()에서 처음 사용하는 경우)
        // DG.Tweening.DOTween.Init(); // 이 부분은 보통 프로젝트 시작 시 한 번만 호출하면 됩니다.

        // 결과 패널의 Animator 컴포넌트 가져오기
        giftBoxAnimator = giftBox.GetComponent<Animator>();
        if (giftBoxAnimator == null)
        {
            Debug.LogError("GiftBox에 Animator 컴포넌트가 없습니다!");
        }

        // 결과 패널의 Animator 컴포넌트 가져오기
        resultPanelAnimator = resultPanel.GetComponent<Animator>();
        if (resultPanelAnimator == null)
        {
            Debug.LogError("ResultPanel에 Animator 컴포넌트가 없습니다!");
        }

        gachaButton.onClick.AddListener(OnGachaButtonClick);
        resultPanel.SetActive(false); // 시작 시 결과 패널 숨기기
    }
    
    void OnGachaButtonClick()
    {
        Debug.Log("뽑기 버튼 클릭!");
        gachaButton.interactable = false; // 뽑기 중에는 버튼 비활성화
        resultPanel.SetActive(false); // 이전 결과 숨기기

        giftBoxAnimator.SetTrigger("Openned");
        // 1. 애니메이터를 이용한 뽑기 연출 (예: 룰렛 돌아가는 애니메이션 등)
        // 여기서는 간단하게 바로 결과물 등장으로 넘어갑니다.
        // 만약 뽑기 중간 연출이 있다면 resultPanelAnimator.SetTrigger("StartGacha") 등으로 시작.
        // 그리고 애니메이션 끝나는 시점에 StartCoroutine(ShowGachaResult()); 호출

        // 지금은 바로 결과물 보여주기 시작
        StartCoroutine(ShowGachaResult());
    }

    IEnumerator ShowGachaResult()
    {
        // 2. 랜덤 아이템 선택
        int randomIndex = Random.Range(0, itemSprites.Length);
        Sprite selectedSprite = itemSprites[randomIndex];
        string selectedName = itemNames[randomIndex];

        resultImage.sprite = selectedSprite;
        resultText.text = selectedName;

        // 3. DOTween을 이용한 결과물 등장 애니메이션
        // 결과 패널을 다시 활성화 (Animator 상태가 "Idle"에서 "Reveal"로 변경)
        resultPanel.SetActive(true);

        // Animator를 이용해 결과 패널 등장 애니메이션 시작
        if (resultPanelAnimator != null)
        {
            resultPanelAnimator.SetTrigger("RevealResult"); // Animator의 Trigger 이름을 "RevealResult"로 설정
        }

        // 결과물이 나타나는 동안 대기
        // Animator의 RevealResult 애니메이션 길이에 맞춰 조정하거나, DOTween 애니메이션과 병행
        yield return new WaitForSeconds(revealDuration); // Animator 애니메이션 길이에 맞춰 대기

        // 4. DOTween을 이용한 '빠밤' 스케일 펀치 및 페이드 인 연출 (Animator와 병행 가능)
        // 결과물 이미지와 텍스트가 서서히 나타나고 커지는 효과
        resultImage.color = new Color(1, 1, 1, 0); // 투명하게 시작
        resultText.color = new Color(1, 1, 1, 0);

        // DOTween 시퀀스 생성
        Sequence revealSequence = DOTween.Sequence();

        // 이미지와 텍스트 페이드 인
        revealSequence.Append(resultImage.DOFade(1, revealDuration * 0.5f));
        revealSequence.Join(resultText.DOFade(1, revealDuration * 0.5f));

        // '빠밤' 스케일 펀치 효과
        revealSequence.Append(resultImage.transform.DOPunchScale(Vector3.one * scalePunchStrength, revealDuration * 0.5f, 5, 0.5f));
        revealSequence.Join(resultText.transform.DOPunchScale(Vector3.one * scalePunchStrength, revealDuration * 0.5f, 5, 0.5f));

        // 이펙트 생성 (결과물 등장 직후)
        yield return new WaitForSeconds(effectDelay); // 결과물이 조금 드러난 후 이펙트 발생
        if (effectPrefab != null)
        {
            GameObject effectInstance = Instantiate(effectPrefab, Vector3.zero + Vector3.forward * 5.0f, Quaternion.identity, resultPanel.transform);
            ParticleSystem effectParticleSystem = effectInstance.GetComponent<ParticleSystem>();
            if (effectParticleSystem != null)
            {
                effectParticleSystem.Play();
            }

            // 파티클 시스템이라면 일정 시간 후 스스로 사라지게 하거나, 스크립트에서 Destroy(effectInstance, effectLifetime); 처리
            Destroy(effectInstance, 2f); // 2초 후 이펙트 삭제
        }

        // 시퀀스 재생 대기
        yield return revealSequence.WaitForCompletion();

        gachaButton.interactable = true; // 뽑기 종료 후 버튼 활성화
    }
}