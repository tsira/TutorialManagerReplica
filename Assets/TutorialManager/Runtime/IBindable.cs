using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IBindable
{
    List<Binding> bindings { get; }
    string uniqueId { get; }
}
