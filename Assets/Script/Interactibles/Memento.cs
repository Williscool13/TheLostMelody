using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memento : MonoBehaviour, IInteractible
{
    public bool Interactable { get; set; }

    public void Interact() {
        // picks it up?
    }
    public void Highlight(bool value) {
        // show that I'm the currently interactible target
    }

    // Start is called before the first frame update
    void Start()
    {
        this.Interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
