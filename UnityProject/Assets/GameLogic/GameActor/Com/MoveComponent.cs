using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    public class InputLogicCmp : GameActorCmp
    {
        private Vector2 m_input;
        private bool m_hasManualInput = false; // 标记是否有手动设置的输入

        public void SetInput(Vector2 input)
        {
            m_input = input;
            m_hasManualInput = true;
        }

        public virtual Vector2 GetInput()
        {
            // 如果有手动设置的输入，优先使用手动输入
            if (m_hasManualInput)
            {
                return m_input;
            }
            
            // 否则从Unity Input获取
            //GetAxis 需要急停，GetAxisRaw 不需要
            return UnityEngine.Input.GetAxisRaw("Horizontal") * Vector2.right +
                   UnityEngine.Input.GetAxisRaw("Vertical") * Vector2.up;
        }
        
        /// <summary>
        /// 清除手动输入，恢复使用Unity Input
        /// </summary>
        public void ClearManualInput()
        {
            m_hasManualInput = false;
            m_input = Vector2.zero;
        }

        public Vector2 GetMouseWorldPosition()
        {
            return Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        public Vector2 GetMousWorldDirection()
        {
            if (Actor.Transform == null) return Vector2.zero;
            return (GetMouseWorldPosition() - (Vector2)Actor.Transform.position).normalized;
        }
    }

    public class MoveLogicCmp : GameActorCmp
    {
        public InputLogicCmp input { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 MoveDirection { get; set; }
        public float Velocity { get; set; }
        
        public bool IsMoving => RunTimeSpeed > 0;
        
        public float RunTimeSpeed { get; set; }
        public bool IsStrict4Direction { get; set; } = true;

        public override void OnInit()
        {
            base.OnInit();
            input = GetComponent<InputLogicCmp>();
            Position = Actor.Transform != null ? Actor.Transform.position : Vector2.zero;
            RunTimeSpeed = 0;
        }

        float epsilon = 0.01f;
        public override void OnUpdate()
        {
            // 优先从 UnitConfig 读取速度，如果没有则从 NumericComponent 获取
            var unitComponent = Actor.GetComponent<UnitComponent>();
            if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
            {
                Velocity = unitComponent.Config.MoveSpeed;
            }
            else
            {
                Velocity = Actor.NumericComponent.GetAsFloat(NumericType.Speed);
            }

            Vector2 direction = Vector2.zero;

            if (input == null)
                return;

            direction = input.GetInput();

            if (Mathf.Abs(direction.x) < epsilon && Mathf.Abs(direction.y) < epsilon)
            {
                RunTimeSpeed = 0;
                return;
            }

            RunTimeSpeed = Velocity;
            
            if (IsStrict4Direction)
            {
                direction = GetFourDirection(direction);
            }
            
            Move(direction);
        }

        protected Vector2 GetFourDirection(Vector2 inputDir)
        {
            // 目标方向（初始化为零向量）
            Vector2 targetDir = Vector2.zero;

            // 比较水平/垂直分量的绝对值，取较大的方向
            if (Mathf.Abs(inputDir.x) > Mathf.Abs(inputDir.y)) 
            {
                targetDir = inputDir.x > 0 ? Vector2.right : Vector2.left; // 右或左
            } 
            else 
            {
                targetDir = inputDir.y > 0 ? Vector2.up : Vector2.down; // 上或下
            }
            return targetDir;
        }

        protected virtual void Move(Vector2 direction)
        {
            MoveDirection = direction.normalized;
            
            Position += direction * (Velocity * Time.deltaTime);
            
            Actor.SetPosition(Position);
        }

        public void SetVelocity(float velocity)
        {
            Velocity = velocity;
        }
    }
    
    public class SimplePathFindingLogicCmp : MoveLogicCmp
    {
        public List<Vector2> Path { get; set; }
        
        int index = 0;

        public SimplePathFindingLogicCmp()
        {
            Path = ActorMgr.Instance.GetCurentLevelPathNodes();
        }

        public override void OnUpdate()
        {
            //计算得出下一步的位置
            // 优先从 UnitConfig 读取速度，如果没有则从 NumericComponent 获取
            var unitComponent = Actor.GetComponent<UnitComponent>();
            if (unitComponent != null && unitComponent.IsConfigValid && unitComponent.Config != null)
            {
                Velocity = unitComponent.Config.MoveSpeed;
            }
            else
            {
                Velocity = Actor.NumericComponent.GetAsFloat(NumericType.Speed);
            }

            if( index < Path.Count  )
            {
                Vector2 direction = (Path[index] - Position).normalized;

                if (IsStrict4Direction)
                {
                    direction = GetFourDirection(direction);
                }
                Move(direction);
                if( (Position - Path[index]).magnitude < 0.1f )
                {
                    index++;
                }
            }
        }
    }
    
    public class MoveViewCmp : GameActorCmp
    {
        MoveLogicCmp move;

        public override void OnInit()
        {
            move = GetComponent<MoveLogicCmp>();
        }

        public override void OnUpdate()
        {
            if (move == null)
                return;
            if (Actor.Transform != null)
            {
                Actor.Transform.position = move.Position;
            }
        }
    }   

  

    public class DirectionViewCmp : GameActorCmp
    {
        MoveLogicCmp move;

        Vector2 Direction;
        
        SpriteRenderer spriteRenderer;
        public override void OnInit()
        {
            base.OnInit();
            move = GetComponent<MoveLogicCmp>();
            if (Actor.Transform != null)
            {
                spriteRenderer = Actor.Transform.GetComponentInChildren<SpriteRenderer>();
            }
        }

        public float lastX;
        public override void OnUpdate()
        {
            if(!CheckIsEnable(move)) return;
            
            if (spriteRenderer == null) return;
            
            Vector2 direction = move.MoveDirection;
            if(!Mathf.Approximately(direction.x, lastX))
            {
                spriteRenderer.flipX = direction.x < 0;
            }
            
            lastX = move.MoveDirection.x;
        }
    }

    //旋转组件
    public class OrientationViewCmp : GameActorCmp
    {
        
        Vector2 m_target;
        
        float m_rotateSpeed = 10;
        
        public Transform m_transform;
        public void SetTarget(Vector2 target)
        {
            m_target = target;
        }

        public override void OnInit()
        {
            base.OnInit();
            if (Actor.Transform != null)
            {
                m_transform = Actor.Transform.GetComponentInChildren<SpriteRenderer>().transform;
            }
        }

        public override void OnUpdate()
        {
            if (CheckHasRotatedToTarget(m_target))
            {
                return;
            }
            //向右为正方向
            Actor.Transform.right = Vector2.Lerp(Actor.Transform.right, (m_target - (Vector2)Actor.Transform.position).normalized, m_rotateSpeed * Time.deltaTime); 
        }
        
        public bool CheckHasRotatedToTarget(Vector2 target)
        {
            //右方向与目标方向的夹角 小于1度
            if (Actor.Transform == null) return false;
            return Vector2.Angle(Actor.Transform.right, (target - (Vector2)Actor.Transform.position)) < 1;
        }
        
       
    }

   
}