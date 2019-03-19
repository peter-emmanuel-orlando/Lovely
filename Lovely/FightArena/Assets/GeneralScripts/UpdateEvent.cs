using System;
using UnityEngine;

public delegate void AwakeEventHandler(MonoBehaviour sender, EventArgs e);
public delegate void UpdateEventHandler(MonoBehaviour sender, EventArgs e);
public delegate void OnDestroyEventHandler(MonoBehaviour sender, EventArgs e);
