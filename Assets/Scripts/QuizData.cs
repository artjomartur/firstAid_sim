using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuizData", menuName = "FirstAid/QuizData")]
public class QuizData : ScriptableObject
{
    public List<QuestionData> questions = new List<QuestionData>();
}
