using UnityEngine;

public class SnakeCollectable : MonoBehaviour
{
    private SnakeHandler _handler;

    //constructor used by snake handler class when spawned in
    public void Initialize(SnakeHandler handler)
    {
        _handler = handler;
    }

    //following method is called when the snake is collected
    public void Collect()
    {//if statement doesn't work then just destroy the game object
            Destroy(gameObject);
    
    }
    //checks for collision if it's player
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

}