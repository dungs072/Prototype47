using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().SetInteractable(this);
            OnEnterRange();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().SetInteractable(null);
            OnExitRange();
        }
    }

    public virtual void OnEnterRange() { Debug.Log("Enter Range " + gameObject.name); }
    public virtual void OnExitRange() { Debug.Log("Exit Range " + gameObject.name); }
    public abstract void OnInteract();
}
