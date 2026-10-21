
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // =========================
    // 플레이어 기본 설정
    // =========================

    public string playerName = "Player";
    public int hp = 100;

    public float moveSpeed = 5f;
    public float jumpPower = 8f;

    // 낙사 기준
    public float fallLimit = -10f;

    // 시작 위치
    public Vector3 startPosition = new Vector3(0, 20, 0);

    // 목숨
    public int lives = 3;

    // 캐릭터 이미지 오브젝트
    public Transform visual;

    // 공중에서 캐릭터 크기
    public Vector2 airScale = new Vector2(0.8f, 1.2f);


    // =========================
    // UI 설정
    // =========================

    // 목숨 표시 텍스트
    public TMP_Text livesText;

    // 게임 오버 패널
    public GameObject gameOverPanel;


    // =========================
    // 내부 변수
    // =========================

    private Vector2 moveInput;
    private Rigidbody2D rb;

    private bool isGrounded = false;
    private bool isGameOver = false;

    // 1 = 오른쪽, -1 = 왼쪽
    private float facing = 1f;


    // =========================
    // 게임 시작
    // =========================

    void Start()
    {
        // Rigidbody2D 가져오기
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다!");
            enabled = false;
            return;
        }

        // 시작 위치로 이동
        transform.position = startPosition;

        // 초기 상태
        isGrounded = false;
        isGameOver = false;

        // 목숨 UI 초기화
        if (livesText != null)
        {
            livesText.text = "LIFE " + lives;
        }
        else
        {
            Debug.LogError("LivesText가 연결되지 않았습니다!");
        }

        // 게임 오버 패널 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameOverPanel이 연결되지 않았습니다!");
        }

        // 체력 상태 출력
        if (hp >= 70)
        {
            Debug.Log("건강");
        }
        else if (hp >= 30)
        {
            Debug.Log("주의");
        }
        else
        {
            Debug.Log("위험");
        }

        Debug.Log(playerName + " 시작");
        Debug.Log("체력 = " + hp);
        Debug.Log("목숨 = " + lives);
    }


    // =========================
    // 좌우 이동 입력
    // =========================

    void OnMove(InputValue value)
    {
        // 게임 오버 상태에서는 이동 불가능
        if (isGameOver)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();

        // 오른쪽
        if (moveInput.x > 0)
        {
            facing = 1f;
        }

        // 왼쪽
        else if (moveInput.x < 0)
        {
            facing = -1f;
        }
    }


    // =========================
    // 점프
    // =========================

    void OnJump(InputValue value)
    {
        // 게임 오버 상태에서는 점프 불가능
        if (isGameOver)
        {
            return;
        }

        // 땅에 있을 때만 점프
        if (value.isPressed && isGrounded)
        {
            Debug.Log("점프!");

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpPower
            );

            // 공중 상태 변경
            isGrounded = false;
        }
    }


    // =========================
    // 매 프레임 실행
    // =========================

    void Update()
    {
        // -------------------------
        // 좌우 이동
        // -------------------------

        if (!isGameOver)
        {
            transform.Translate(
                Vector3.right *
                moveInput.x *
                moveSpeed *
                Time.deltaTime
            );
        }


        // -------------------------
        // 낙사 감지
        // -------------------------

        if (!isGameOver && transform.position.y < fallLimit)
        {
            LoseLife();
        }


        // -------------------------
        // 캐릭터 방향 / 크기
        // -------------------------

        if (visual != null)
        {
            // 땅에 있을 때
            if (isGrounded)
            {
                visual.localScale = new Vector3(
                    facing,
                    1f,
                    1f
                );
            }

            // 공중에 있을 때
            else
            {
                visual.localScale = new Vector3(
                    facing * airScale.x,
                    airScale.y,
                    1f
                );
            }
        }
    }


    // =========================
    // 목숨 감소 및 UI 업데이트
    // =========================

    void LoseLife()
    {
        // 게임 오버 상태에서는 실행하지 않음
        if (isGameOver)
        {
            return;
        }

        // 목숨 1개 감소
        lives -= 1;

        // 목숨이 0보다 작아지지 않도록 설정
        if (lives < 0)
        {
            lives = 0;
        }

        // 시작 위치로 이동
        transform.position = startPosition;

        // 속도 초기화
        rb.linearVelocity = Vector2.zero;

        // 공중 상태 초기화
        isGrounded = false;

        // 목숨 UI 업데이트
        if (livesText != null)
        {
            livesText.text = "LIFE " + lives;
        }

        Debug.Log("남은 목숨 " + lives);


        // -------------------------
        // 게임 오버
        // -------------------------

        if (lives <= 0)
        {
            isGameOver = true;

            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;

            // 게임 오버 패널 표시
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                Debug.Log("게임 오버 패널 활성화!");
            }
            else
            {
                Debug.LogError(
                    "GameOverPanel이 연결되지 않았습니다!"
                );
            }

            Debug.Log("게임 오버");
        }
    }


    // =========================
    // 바닥에 착지
    // =========================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            Debug.Log("착지");
        }
    }


    // =========================
    // 바닥에서 떨어짐
    // =========================

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;

            Debug.Log("공중");
        }
    }


    // =========================
    // 골인 / 함정 충돌
    // =========================

    void OnTriggerEnter2D(Collider2D other)
    {
        // 게임 오버 상태에서는 충돌 처리 안 함
        if (isGameOver)
        {
            return;
        }

        // 골인 지점
        if (other.CompareTag("Goal"))
        {
            Debug.Log("골인!");
        }

        // 함정 충돌
        else if (other.CompareTag("Trap"))
        {
            LoseLife();
        }
    }
}
