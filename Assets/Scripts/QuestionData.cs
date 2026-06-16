using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "FirstAid/Question")]
public class QuestionData : ScriptableObject
{
    [TextArea(3, 10)]
    public string questionText;
    public string[] answers;
    public int correctAnswerIndex;
}
