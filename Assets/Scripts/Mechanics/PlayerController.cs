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
    //player data class will be converted to JSON when saved
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
            //add score to list
            highscores.Add(score);
            //sort
            Sort();
            //trim to the desired depth
            Trim(5);
        }

        public void Sort()
        {
            //we use linq here instead of a normal sort because default times im using are just 0.
            //the expression sorts from lowest to highest time and places zeroed out times at the end.
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

        //round timer and current best time
        //bestRoundTime will be set in ReadData to the current best
        public float roundTimer = 0f;
        public float bestRoundTime = 0f;

        //deaths this round
        public float instanceDeaths = 0f;

        //player save data
        private PlayerData _data;

        private void Start()
        {
            //init counters/times
            roundTimer = 0;
            instanceDeaths = 0;
            //read data
            ReadData();
        }

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        void WriteData()
        {
            //seralize to json
            string saveData = JsonUtility.ToJson(_data);
            //save in slot
            PlayerPrefs.SetString("PlayerData", saveData);
        }

        void ReadData()
        {
            //seralize default data
            string defaultData = JsonUtility.ToJson(new PlayerData());
            //read data from slot
            string readData = PlayerPrefs.GetString("PlayerData", defaultData);
            //set data
            _data = JsonUtility.FromJson<PlayerData>(readData);
            //set best time.
            bestRoundTime = _data.highscores[0];
        }

        //get score list
        public List<float> GetHighscores() => _data.highscores;

        //used by VictoryEvent()
        private bool CheckScoreForUpdate()
        {
            //add curent score
            _data.AddScore(roundTimer);

            if (roundTimer < bestRoundTime || bestRoundTime == 0)
            {
                //beat score
                bestRoundTime = _data.highscores[0];
                return true;
            }

            //no highscore
            return false;
        }

        //used in PlayerSpawn
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

            //reset round data
            roundTimer = 0;
            instanceDeaths = 0;

            WriteData();
        }

        //called by PlayerDeath Event
        public void DeathEvent()
        {
            //Im using the death event to respawn the player because the animation for death fits a restart animation as well.
            //control will be disabled at this point already because of the player finish event.
            //this check makes sure that its a death and not a restart that is being counted.
            if(controlEnabled)
                instanceDeaths++;

            WriteData();
        }

        //called by PlayerEnteredVictoryZone Event
        public bool VictoryEvent()
        {
            //check if the player surpassed their highscore
            bool gotHigh = CheckScoreForUpdate();
            WriteData();
            return gotHigh;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                //increment timer only if player has control
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