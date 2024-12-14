using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace TempleRun.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float initialPlayerSpeed = 4f;
        [SerializeField] private float maximumPlayerSpeed = 30f;
        [SerializeField] private float PlayerSpeedIncreaseRate = .1f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float initialGravityValue = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip slideAnimationClip;
        [SerializeField] private AnimationClip runAnimationClip;
        [SerializeField] private AnimationClip jumpAnimationClip;
        [SerializeField] private float playerSpeed;
        [SerializeField] private float scoreMuitiplier = 10f;

        private static readonly int RunningHash = Animator.StringToHash("Running");
        private static readonly int SlidingHash = Animator.StringToHash("Sliding");
        private static readonly int JumpingHash = Animator.StringToHash("Jumping");

        private float gravity;
        private Vector3 movementDirection = Vector3.forward;
        private Vector3 playerVelocity;
        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;
        private CharacterController controller;

        private bool sliding = false;
        private bool jumping = false;
        private float score = 0;

        [SerializeField] private UnityEvent<Vector3> turnEvent;
        [SerializeField] private UnityEvent<int> gameOverEvent;
        [SerializeField] private UnityEvent<int> scoreUpdateEvent;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];
        }

        private void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            slideAction.performed += PlayerSlide;
            jumpAction.performed += PlayerJump;
        }

        private void OnDisable()
        {
            turnAction.performed -= PlayerTurn;
            slideAction.performed -= PlayerSlide;
            jumpAction.performed -= PlayerJump;
        }

        private void Start()
        {
            playerSpeed = initialPlayerSpeed;
            gravity = initialGravityValue;
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found!");
            }
            else
            {
                SetRunningAnimation(true);
                Debug.Log("Starting running animation");
            }
            controller.stepOffset = 0.05f;
        }

        private void SetRunningAnimation(bool isRunning)
        {
            animator.SetBool(RunningHash, isRunning);
            Debug.Log($"Setting running animation to: {isRunning}");
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                GameOver();
                return;
            }
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition.Value);
        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1) || (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        private void Turn(float turnValue, Vector3 turnPosition)
        {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            controller.enabled = false;
            transform.position = tempPlayerPosition;
            controller.enabled = true;
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            movementDirection = transform.forward.normalized;
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if (!sliding)
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            sliding = true;
            animator.SetBool(RunningHash, false);  // 달리기 애니메이션 중지
            animator.SetBool(SlidingHash, true);   // 슬라이드 애니메이션 시작
            Debug.Log("Starting slide animation: " + animator.GetBool(SlidingHash));

            yield return new WaitForSeconds(slideAnimationClip.length);

            animator.SetBool(SlidingHash, false);  // 슬라이드 애니메이션 종료
            animator.SetBool(RunningHash, true);   // 달리기 애니메이션 재개
            sliding = false;
            Debug.Log("Ending slide animation: " + animator.GetBool(SlidingHash));

        }
        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (!jumping)
            {
                StartCoroutine(Jump());
            }
        }

        private IEnumerator Jump()
        {
            jumping = true;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
            controller.Move(playerVelocity * Time.deltaTime);

            animator.SetBool(RunningHash, false);  // 달리기 애니메이션 중지
            animator.SetBool(JumpingHash, true);   // 점프 애니메이션 시작
            Debug.Log("Starting jump animation: " + animator.GetBool(JumpingHash));

            yield return new WaitForSeconds(jumpAnimationClip.length);

            animator.SetBool(JumpingHash, false);  // 점프 애니메이션 종료
            animator.SetBool(RunningHash, true);   // 달리기 애니메이션 재개
            jumping = false;
            Debug.Log("Ending jump animation: " + animator.GetBool(JumpingHash));
        }

        //애니메이션 로그
        

        private void Update()
        {
            //
            if (!IsGrounded(20f))
            {
                GameOver();
                return;
            }

            score += scoreMuitiplier * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)score);

            controller.Move(transform.forward * playerSpeed * Time.deltaTime);

            if (IsGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            if (playerSpeed < maximumPlayerSpeed)
            {
                playerSpeed += Time.deltaTime * PlayerSpeedIncreaseRate;
                gravity = initialGravityValue - playerSpeed;
                if (animator.speed < 1.25f)
                {
                    animator.speed += (1 / playerSpeed) + Time.deltaTime;
                }
            }

            // 매 프레임마다 달리기 애니메이션 상태 확인
            if (!sliding && !jumping && !animator.GetBool(RunningHash))
            {
                SetRunningAnimation(true);
                Debug.Log("Forcing running animation in Update");
            }

        }

        private bool IsGrounded(float length = 0.5f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= controller.height / 2f;
            raycastOriginFirst.y += 0.1f;
            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * 0.2f;
            raycastOriginSecond += transform.forward * 0.2f;

            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) ||
                Physics.Raycast(raycastOriginSecond, Vector3.down, out hit, length, groundLayer))
            {
                return true;
            }

            return false;
        }

        private void GameOver()
        {
            gameOverEvent.Invoke((int)score);
            enabled = false;
        }
    }
}