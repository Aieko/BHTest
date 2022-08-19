using UnityEngine;
using Mirror;
using System.Collections;

namespace BHTest
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        public CharacterController characterController;

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        public float maxTurnSpeed = 100f;
        public float dashSpeed = 5f;
        public float dashingTime = 0.2f;
        public float dashCoolDown = 2f;

        [Header("Immortality Settings")]
        public float immortalityTime;

        [Header("Diagnostics")]
        public float horizontal;
        public float vertical;
        public float jumpSpeed;
        public bool isGrounded = true;
        public bool isFalling;
        public Vector3 velocity;

        [SyncVar]
        private bool _canDash = true;
        [SyncVar]
        private bool _dashing;
        [SyncVar]
        private bool isImmortal = false;
        private Material _characterMaterial;
        private Color _characterColor;
        private PlayerScore _playerScore;
        [SerializeField] private BoxCollider hitBoxCollider;

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<NetworkTransform>().clientAuthority = true;

            //hitBoxCollider.enabled = true;
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        private void Start()
        {
            _characterMaterial = GetComponentInChildren<Renderer>().material;
            _characterColor = _characterMaterial.color;
            _playerScore = GetComponent<PlayerScore>();
        }

        void Update()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            if (isGrounded)
                isFalling = false;

            if(Input.GetKeyDown(KeyCode.Mouse0) && _canDash)
            {
                if (isServer) StartCoroutine(Dash());
                else CmdDash();
            }

            if ((isGrounded || !isFalling) && jumpSpeed < 1f && Input.GetKey(KeyCode.Space))
            {
                jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
            }
            else if (!isGrounded)
            {
                isFalling = true;
                jumpSpeed = 0;
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (jumpSpeed > 0)
                characterController.Move(direction * Time.fixedDeltaTime);
            else
                characterController.SimpleMove(direction);

            if (_dashing) characterController?.Move(transform.forward * dashSpeed * Time.fixedDeltaTime);

            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
        }

        private IEnumerator Dash()
        {
            if (_dashing) yield return null;

            _dashing = true;
            _canDash = false;

            yield return new WaitForSeconds(dashingTime);

            _dashing = false;
           

            yield return new WaitForSeconds(dashCoolDown - dashingTime);

            _canDash = true;

        }

        private IEnumerator WasHit(GameObject target)
        {
            var playerController = target.GetComponent<PlayerController>();
            playerController._characterMaterial.color = Color.red;
            
            yield return new WaitForSeconds(immortalityTime);

            playerController._characterMaterial.color = playerController._characterColor;
            
        }

        #region Server

        [Command]
        private void CmdDash()
        {
            StartCoroutine(Dash());  
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && gameObject != other.gameObject)
            {
                if (!_dashing) return;

                var player = other.GetComponent<PlayerController>();
                
                if (player.isImmortal) return;
               
                _playerScore.AddScore();
                RpcWasHit(player.gameObject);
                StartCoroutine(BecomeImmortal(other.gameObject));
               
            }
        }

        private IEnumerator BecomeImmortal(GameObject playerGO)
        {
            var player = playerGO.GetComponent<PlayerController>();

            player.isImmortal = true;

            yield return new WaitForSeconds(immortalityTime);

            player.isImmortal = false;
        }

        #endregion

        #region Client

        [ClientRpc]
        private void RpcWasHit(GameObject player)
        {
            StartCoroutine(WasHit(player));
        }

        #endregion
    }
}
