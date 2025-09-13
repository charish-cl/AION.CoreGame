using System.Collections.Generic;
using UnityEngine;
using AION.CoreFramework;

namespace GameLogic
{
    public class InputLogicCmp : GameActorCmp
    {
        Vector2 Input;

        public void SetInput(Vector2 input)
        {
            Input = input;
        }

        public virtual Vector2 GetInput()
        {
            return UnityEngine.Input.GetAxis("Horizontal") * Vector2.right +
                   UnityEngine.Input.GetAxis("Vertical") * Vector2.up;
        }

        public Vector2 GetMouseWorldPosition()
        {
            return Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        public Vector2 GetMousWorldDirection()
        {
            return (GetMouseWorldPosition() - (Vector2)Actor.m_transform.position).normalized;
        }
    }

    public class MoveLogicCmp : GameActorCmp
    {
        public InputLogicCmp input { get; set; }

        public Vector2 Position { get; set; }

        public Vector2 MoveDirection { get; set; }
        public float Velocity { get; set; } = 1;


        public bool IsStrict4Direction { get; set; } = true;

        public override void OnInit()
        {
            base.OnInit();
            input = GetComponent<InputLogicCmp>();
            Position = Actor.m_transform.position;
        }

        public override void OnUpdate()
        {
            Vector2 direction = Vector2.zero;

            if (input == null)
                return;

            direction = input.GetInput();
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
            Path = SceneMgr.Instance.GetCurentLevelPathNodes();
        }

        public override void OnUpdate()
        {
            //计算得出下一步的位置
            
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
            base.OnInit();
            move = GetComponent<MoveLogicCmp>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (move == null)
                return;
            Actor.m_transform.position = move.Position;
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
            spriteRenderer = Actor.m_transform.GetComponent<SpriteRenderer>();
        }

        public float lastX;
        public override void OnUpdate()
        {
            base.OnUpdate();
            
            if(!CheckIsEnable(move)) return;
            
            
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
            m_transform = Actor.m_transform.GetComponentInChildren<SpriteRenderer>().transform;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (CheckHasRotatedToTarget(m_target))
            {
                return;
            }
            //向右为正方向
            Actor.m_transform.right = Vector2.Lerp(Actor.m_transform.right, (m_target - (Vector2)Actor.m_transform.position).normalized, m_rotateSpeed * Time.deltaTime); 
        }
        
        public bool CheckHasRotatedToTarget(Vector2 target)
        {
            //右方向与目标方向的夹角 小于1度
            return Vector2.Angle(Actor.m_transform.right, (target - (Vector2)Actor.m_transform.position)) < 1;
        }
        
       
    }

   
}