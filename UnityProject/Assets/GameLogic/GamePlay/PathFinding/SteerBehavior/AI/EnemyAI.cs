using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(ObstacleDetector))]
[RequireComponent(typeof(TargetDetector))]
[RequireComponent(typeof(SeekBehaviour))]
[RequireComponent(typeof(ObstacleAvoidanceBehaviour))]
[RequireComponent(typeof(AIData))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours;
    [SerializeField]
    private List<Detector> detectors;
    [SerializeField]
    private AIData aiData;
    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;
    [SerializeField]
    private float attackDistance = 0.5f;
    public float speed=4;
    //Inputs sent from the Enemy AI to the Enemy controller
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    private Vector2 movementInput;

    //计算移动方向
    private ContextSolver context;
    
    bool following = false;
    
    
    private void Start()
    {
        steeringBehaviours = GetComponentsInChildren<SteeringBehaviour>().ToList();
        detectors = GetComponentsInChildren<Detector>().ToList();
        context = gameObject.AddComponent<ContextSolver>();
        //Detecting Player and Obstacles around
        InvokeRepeating("Detection", 0, detectionDelay);
    }

    public void Move(Vector2 mov)
    {
        transform.Translate(mov*Time.deltaTime*speed);
    }
    //Detecting Player and Obstacles around
    private void Detection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        //Enemy AI movement based on Target availability
        if (aiData.currentTarget != null)
        {
            //Looking at the Target
            if (following == false)
            {
                following = true;
                StartCoroutine(ChaseAndAttack());
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            //Target acquisition logic
            aiData.currentTarget = aiData.targets[0];
        }
        //Moving the Agent
        Move(movementInput);
        
    }

    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            //Stopping Logic
            Debug.Log("Stopping");
            movementInput = Vector2.zero;
            following = false;
            yield break;
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (distance < attackDistance)
            {
                movementInput = Vector2.zero;
            }
            else
            {
                //Chase logic
                movementInput = context.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }

        }

    }
}
