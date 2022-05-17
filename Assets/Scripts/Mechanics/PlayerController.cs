using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    [System.Serializable]
    public class PlayerData
    {
        public List<float> highscores;

        public PlayerData()
        {
            highscores = new List<float>();
            for (int i = 0; i < 5; i++) highscores.Add(0);
        }

        public void AddScore(float score)
        {
            highscores.Add(score);
            Sort();
            Trim(5);
        }

        public void Sort()
        {
            highscores = highscores.OrderBy(x => x == 0).ThenBy(x => x).ToList();

        }

        //trim list to the amount highscores we want to track
        public void Trim(int amount)
        {
            highscores.RemoveRange(amount, (highscores.Count)-amount);
        }
    }


    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        /// <summary>
        /// Period of time where player may jump still while being "ungrounded"
        /// </summary>
        public float jumpGracePeriod = 0.2f;
        private float jumpGracePeriodTimer = 0;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        public float roundTimer = 0f;
        public float bestRoundTime = 0f;
        public float instanceDeaths = 0f;

        private PlayerData _data;

        private void Start()
        {
            roundTimer = 0;
            instanceDeaths = 0;

            Read();
        }

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        void Write()
        {
            string saveData = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString("PlayerData", saveData);
        }

        void Read()
        {
            string defaultData = JsonUtility.ToJson(new PlayerData());
            string readData = PlayerPrefs.GetString("PlayerData", defaultData);
            _data = JsonUtility.FromJson<PlayerData>(readData);
            bestRoundTime = _data.highscores[0];
        }

        public List<float> GetHighscores() => _data.highscores;

        public bool CheckScoreForUpdate()
        {
            _data.AddScore(roundTimer);

            if (roundTimer < bestRoundTime || bestRoundTime == 0)
            {
                bestRoundTime = _data.highscores[0];
                return true;
            }

            return false;
        }

        public void SpawnEvent()
        {
            collider2d.enabled = true;
            controlEnabled = false;
            if (audioSource && respawnAudio)
                audioSource.PlayOneShot(respawnAudio);
            health.Increment();
            Teleport(model.spawnPoint.transform.position);
            jumpState = PlayerController.JumpState.Grounded;
            animator.SetBool("dead", false);
            roundTimer = 0;

            Write();
        }

        public void DeathEvent()
        {
            instanceDeaths++;

            Write();
        }

        public bool VictoryEvent()
        {
            bool gotHigh = CheckScoreForUpdate();
            Write();
            return gotHigh;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                roundTimer += Time.deltaTime;

                move.x = Input.GetAxis("Horizontal");

                if ((jumpState == JumpState.Grounded) && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();

            base.Update();
        }

        void UpdateJumpState()
        {
            //if grounded we'll start our timer otherwise reset to 0
            jumpGracePeriodTimer = (IsGrounded) ? jumpGracePeriodTimer + Time.deltaTime : 0;

            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            //check if within grace period
            bool inGracePeriod = (jumpGracePeriodTimer <= jumpGracePeriod);
            //check if player is grounded or ungrounded and within our grace period.
            bool canJumpThisFrame = (IsGrounded) || (inGracePeriod && jumpState != JumpState.PrepareToJump);
            
            if (jump && canJumpThisFrame)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}