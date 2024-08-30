using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BrickGameManager : MonoBehaviour
{
    [SerializeField] private int levelIndex = 0;
    [SerializeField] private float timeLeft = 100;
    [SerializeField] private bool isStarted = false;
    [SerializeField] private bool isEnded = false;
    [SerializeField] private int score = 0;
    [SerializeField] private LevelProperties[] levels;

    private int currentQuestionIndex = -1;

    [SerializeField] private Transform t_levelBtnHolder;
    [SerializeField] private GameObject pf_btn;
    [SerializeField] private Transform t_description;

    [SerializeField] private GameObject go_question;
    [SerializeField] private TextMeshProUGUI meaningText;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform t_answers;

    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void DisplayLevelBtn()
    {
        t_description.gameObject.SetActive(false);
        for (int i = 0; i < levels.Length; i++)
        {
            int _levelNumber = i + 1;
            GameObject go = Instantiate(pf_btn, t_levelBtnHolder);
            go.GetComponentInChildren<TextMeshProUGUI>().text = "Level" + (_levelNumber).ToString();
            go.GetComponent<Button>().onClick.AddListener(() => StartLevel(_levelNumber));
        }
    }

    void NextQuestion()
    {
        if (isEnded) return;
        currentQuestionIndex = (currentQuestionIndex + 1) % levels[levelIndex].questions.Length;
        image.sprite = sprites[0];

        Question q = levels[levelIndex].questions[currentQuestionIndex];
        displayText.text = q.leftPart + "[   ]" + q.rightPart;
        meaningText.text = "";
        int x = UnityEngine.Random.Range(0, t_answers.childCount);
        int c = 0;
        for (int i = 0; i < t_answers.childCount; i++)
        {
            t_answers.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
            if (i==x)
            {
                t_answers.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = q.correctWord;
                t_answers.GetChild(i).GetComponent<Button>().onClick.AddListener(() => StartCoroutine(AnswerQuestion(true, q.correctWord)));
            }
            else
            {
                string s = q.wrongWords[c % q.wrongWords.Length];
                t_answers.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = q.wrongWords[c%q.wrongWords.Length];
                t_answers.GetChild(i).GetComponent<Button>().onClick.AddListener(() => StartCoroutine(AnswerQuestion(false, s)));
                c++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted || isEnded) return;
        timeText.text = ((int)timeLeft).ToString();
        if (timeLeft < 0)
        {
            EndGame();
        }
        timeLeft -= Time.deltaTime;
    }

    private IEnumerator AnswerQuestion(bool isCorrect, string s)
    {
        if (isEnded) yield break;
        for (int i = 0; i < t_answers.childCount; i++)
        {
            t_answers.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
        }
        Question q = levels[levelIndex].questions[currentQuestionIndex];
        if (isCorrect)
        {
            Debug.Log("correct");
            image.sprite = sprites[1];
            score++;
            scoreText.text = $"score: {score}";
            meaningText.text = q.meaning;
        }
        else
        {
            Debug.Log("wrong");
            image.sprite = sprites[2];
            meaningText.text = "這不是詞語";
        }
        displayText.text = q.leftPart + s + q.rightPart;

        float waitTime = 1f;
        yield return new WaitForSeconds(waitTime);

        NextQuestion();
    }

    public void StartLevel(int _levelNumber)
    {
        if (isStarted) return;
        t_levelBtnHolder.gameObject.SetActive(false);
        levelIndex = _levelNumber - 1;
        timeLeft = levels[levelIndex].gameTime;
        isStarted = true;

        go_question.SetActive(true);
        image.gameObject.SetActive(true);
        NextQuestion();
    }

    private void EndGame()
    {
        isEnded = true;
        isStarted = false;
        timeText.text = "Time's up";
        Debug.Log($"game eneded. score: {score}");
        if (levels[levelIndex].scoreRequired <= score)
        {
            Debug.Log("won");
            image.sprite = sprites[3];
        }
        else
        {
            Debug.Log("lose");
            image.sprite = sprites[4];
        }
    }

    [Serializable]
    public class LevelProperties
    {
        public float gameTime = 60f;
        public int scoreRequired = 10;
        public Question[] questions;
    }

    [Serializable]
    public class Question
    {
        public string correctWord;
        public string leftPart;
        public string rightPart;
        public string meaning;
        public string[] wrongWords;
    }
}
