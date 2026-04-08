using UnityEngine;

public class EnemySightRotation : MonoBehaviour
{
    public EnemyController Enemy;
    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        Handanimation();
    }


    private void Handanimation()
    {
        anim.SetFloat("xVelocity", Enemy.velocityX);
        anim.SetFloat("yVelocity", Enemy.velocityY);
    }
}
