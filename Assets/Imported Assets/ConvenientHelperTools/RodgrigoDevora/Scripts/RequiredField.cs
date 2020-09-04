using UnityEngine;

public class RequiredField : PropertyAttribute
{
    public Color color;

    public RequiredField()
    {
        this.color = Color.red;
    }
}
