using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BoardParameters", order = 1)]
public class BoardParametersScriptableObject : ScriptableObject
{
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    [SerializeField] private int colorCount;

    public int RowCount => rowCount;
    public int ColumnCount => columnCount;
    public int ColorCount => colorCount;
}
