using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractible
{

    public bool Interactable { get; set; }
    public void Interact();

    public void Highlight(bool val);
}
