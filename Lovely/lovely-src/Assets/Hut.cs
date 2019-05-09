using UnityEngine;

public class Hut : Spawnable
{
    private void OnDrawGizmosSelected()
    {
        base.Start();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
