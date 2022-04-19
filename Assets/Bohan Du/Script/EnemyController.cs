using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    //攻击距离范围
    private const int ATTACK_DISTANCE = 4;

    //吸引怪物距离
    private const int RUN_TO_ROLE_DISTANCE = 50;

    //攻击对应的生命减少值
    private const int BlOOD_REDUCE_ATTACK1 = 6;
    private const int BlOOD_REDUCE_ATTACK2 = 8;
    private const int BlOOD_REDUCE_ATTACK3 = 10;

    //怪物当前的状态
    private const int STATE_IDLE = 1;
    private const int STATE_RUN = 2;
    private const int STATE_ATTACK = 3;
    private const int STATE_DEAD = 4;
    private const int UNDER_ATTACK = 5;

    //当前状态
    private int currentState;
    //动画
    private Animation ani;
    //主角
    public GameObject role;
    //寻路
    Vector3 destination;
    UnityEngine.AI.NavMeshAgent agent;

    //是否被攻击
    private bool isAttacked = false;
    //是否已经被主角吸引了仇恨
    private bool isAttacking = false;

    /*
    public class Common
    {
        //移动速度
        public const int MOVE_SPEED = 1;
        //旋转速度
        public const int ROTATE_SPEED = 20;
    }
    */

    // Use this for initialization
    void Start () {
        ani = GetComponent<Animation>();
        // Cache agent component and destination
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        destination = agent.destination;
        role = GameObject.Find("FPSController");
    }
	
	// Update is called once per frame
	void Update () {
        checkState();
        checkAttack();
        handlerAction();
    }

    //检测怪物状态
    private void checkState()
    {
        if (Vector3.Distance(role.transform.position, transform.position) <= RUN_TO_ROLE_DISTANCE
&& Vector3.Distance(role.transform.position, transform.position) > ATTACK_DISTANCE && false == isAttacked)
        {
            currentState = STATE_RUN;
            isAttacking = true;  //设置怪物仇恨被激活
        }
        else if (Vector3.Distance(role.transform.position, transform.position) <= ATTACK_DISTANCE && false == isAttacked)
        {
            currentState = STATE_ATTACK;
            //isAttacking = true;  //设置怪物仇恨被激活
        }
        else if (Vector3.Distance(role.transform.position, transform.position) > ATTACK_DISTANCE && true == isAttacking && false == isAttacked)
        {
            currentState = STATE_RUN;
        }
        else if (true == isAttacked)
        {
            currentState = UNDER_ATTACK;
        }
        else
        {
            currentState = STATE_IDLE;
        }
    }

    //是否已经被吸引了仇恨
    private void checkAttack()
    {
        if (true == isAttacking)
        {
            //transform.LookAt(role.transform);
            if (Vector3.Distance(role.transform.position, transform.position) > ATTACK_DISTANCE)
            {
                run();
            }
        }
    }

    //朝主角跑来
    private void run()
    {
        //ani.Play("run");
        ani.CrossFade("run", 0.1f, PlayMode.StopAll);
        /*
        if (false == ani.isPlaying)
        {
            ani.CrossFade("run", 0.1f, PlayMode.StopAll);
        }
        */
        //transform.LookAt(role.transform);
        //Returns this vector with a magnitude of 1 (Read Only).
        //When normalized, a vector keeps the same direction but its length is 1.0.
        //Vector3 dir = (role.transform.position - transform.position).normalized;//追击方向
        //transform.Translate(-dir * Common.MOVE_SPEED * Time.deltaTime);//不停地移动
        //Debug.Log("runToYou");
        // Update destination if the target moves one unit
        destination = role.transform.position;
        agent.destination = destination;
    } 

    //根据状态处理处理怪物动作
      private void handlerAction()
    {
        switch (currentState)
        {
            case STATE_IDLE:
                ani.Play("dance");
                //ani.Play("idle");
                break;
            case STATE_RUN:
                run();
                break;
            case STATE_ATTACK:
                //上一次攻击完了之后才进行下一次攻击
                if (false == ani.isPlaying)
                {
                    attack();
                }
                break;
            case STATE_DEAD:
                dead();
                break;
            case UNDER_ATTACK:
                    ani.Play("die");
                    //ani.CrossFade("die", 0.1f, PlayMode.StopAll);
                break;
            default:
                Debug.Log("error state = " + currentState);
                break;
        }
    }

    //攻击
    private void attack()
    {
        isAttacking = true;
        int attackIndex = Random.Range(1, 4);
        switch (attackIndex)
        {
            case STATE_IDLE:
                ani.Play("attack");
                //告诉主角生命减少量 
                //Calls the method named methodName on every MonoBehaviour in this game object.
                role.SendMessage("reduceBlood", BlOOD_REDUCE_ATTACK1);
                break;
            case STATE_RUN:
                ani.Play("attack");
                //告诉主角生命减少量
                role.SendMessage("reduceBlood", BlOOD_REDUCE_ATTACK2);
                break;
            case STATE_ATTACK:
                ani.Play("attack");
                //告诉主角生命减少量
                role.SendMessage("reduceBlood", BlOOD_REDUCE_ATTACK3);
                break;
            default:
                Debug.Log("error state = " + currentState);
                break;
        }
    }

    //死亡
    public void dead()
    {
        isAttacked = true;
        agent.destination = transform.position;
        isAttacking = false;
        agent.enabled = false;
        float time = 1.6f;
        //ani.Play("die");
        Destroy(gameObject, time);
    }
}