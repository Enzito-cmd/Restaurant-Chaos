using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class AngryChaseState : ClientState
    {
        private const string YellTrigger = "Yell";
        private const float MovingStartThreshold = 0.25f;
        private const float MovingStopThreshold = 0.1f;
        private const float TurnSpeed = 360f;
        private const float ChaseAngularSpeed = 360f;
        private const float YellEnterTimeout = 0.5f;
        private const float YellMaxDuration = 6f;

        private bool isYelling;
        private bool yellEntered;
        private bool isMovingForAnimation;
        private float yellStartTime;
        private float nextYellTime;

        public AngryChaseState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            SoundManager.Instance?.PlaySound(SoundType.ClientAngry);

            client.NotifyLeftQueue();
            client.OrderDisplay.Clear();

            if (client.CurrentChair != null)
            {
                client.CurrentChair.SetOccupied(false);
                client.CurrentChair = null;
            }

            if (client.Agent != null)
            {
                client.Agent.enabled = true;
                client.Agent.isStopped = false;
                client.Agent.angularSpeed = ChaseAngularSpeed;
            }

            nextYellTime = Time.time;
        }

        public override void Tick()
        {
            if (client.Player == null || client.Agent == null || !client.Agent.isOnNavMesh)
            {
                return;
            }

            if (isYelling)
            {
                UpdateYell();
                return;
            }

            float chaseSpeed = client.Config.moveSpeed * client.Config.chaseSpeedMultiplier;
            bool arrived = client.ApproachTarget(client.Player.position, chaseSpeed, client.Config.chaseStopDistance);

            if (!arrived)
            {
                return;
            }

            FacePlayer();

            if (Time.time >= nextYellTime)
            {
                StartYell();
            }
        }

        private void StartYell()
        {
            isYelling = true;
            yellEntered = false;
            yellStartTime = Time.time;

            client.Agent.isStopped = true;
            client.Agent.velocity = Vector3.zero;

            if (client.Animator != null)
            {
                client.Animator.SetTrigger(YellTrigger);
            }
        }

        private void UpdateYell()
        {
            FacePlayer();

            if (HasYellFinished())
            {
                isYelling = false;
                nextYellTime = Time.time + client.Config.yellCooldown;
                client.Agent.isStopped = false;
            }
        }

        private bool HasYellFinished()
        {
            float elapsed = Time.time - yellStartTime;

            if (client.Animator == null)
            {
                return elapsed >= client.Config.yellDuration;
            }

            if (elapsed >= YellMaxDuration)
            {
                client.Animator.ResetTrigger(YellTrigger);
                return true;
            }

            bool inYellState = IsInYellState();

            if (inYellState)
            {
                yellEntered = true;
                return false;
            }

            if (yellEntered)
            {
                return true;
            }

            if (elapsed >= YellEnterTimeout)
            {
                client.Animator.ResetTrigger(YellTrigger);
                return true;
            }

            return false;
        }

        private bool IsInYellState()
        {
            Animator animator = client.Animator;

            if (animator.GetCurrentAnimatorStateInfo(0).IsName(YellTrigger))
            {
                return true;
            }

            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(YellTrigger);
        }

        private void FacePlayer()
        {
            Vector3 direction = client.Player.position - client.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion wanted = Quaternion.LookRotation(direction);
            client.transform.rotation = Quaternion.RotateTowards(client.transform.rotation, wanted, TurnSpeed * Time.deltaTime);
        }

        public override void UpdateAnimation()
        {
            float velocity = 0f;

            if (client.Agent != null && client.Agent.isOnNavMesh)
            {
                velocity = client.Agent.velocity.magnitude;
            }

            if (isMovingForAnimation)
            {
                if (velocity < MovingStopThreshold)
                {
                    isMovingForAnimation = false;
                }
            }
            else if (velocity > MovingStartThreshold)
            {
                isMovingForAnimation = true;
            }

            if (isMovingForAnimation)
            {
                client.Animator.SetFloat("Speed", 2f);
            }
            else
            {
                client.Animator.SetFloat("Speed", 0f);
            }
        }
    }
}
