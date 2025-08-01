using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Demo {

    internal class Answer : ICloneable {

        #region Attrbuties
        private int answerId;
        private string? answerText;
        #endregion

        #region Properties
        public int AnswerId {
            get { return answerId; }
            set { answerId = value; }
        }

        public string AnswerText {
            get { return answerText ?? "No Answer"; }
            set { answerText = value; }
        }
        #endregion

        #region Constrcutor
        public Answer(int id, string text) {
            AnswerId = id;
            AnswerText = text;
        }
        #endregion

        #region Methods
		public object Clone() {
            return new Answer(this.AnswerId, this.AnswerText);
        }
        public override string ToString() {
            return $"Answer {AnswerId}: {AnswerText}";
        }
        #endregion

    }

    internal abstract class Question {

        #region Attributes
        private string? questionHeader;
        private string? questionBody;
        private double mark;
        public Answer[]? AnswerList { get; set; }
        public Answer? RightAnswer { get; set; }
        #endregion

        #region Constructors
        public Question(string header, string body, double mark) {
            QuestionHeader = header;
            QuestionBody = body;
            Mark = mark;
        }
        public Question() : this("Default Header", "Default Body", 1) { }
        #endregion

        #region Properties
        public double Mark {
            get { return mark; }
            set { mark = value; }
        }

        public string? QuestionBody {
            get { return questionBody ?? "No body"; }
            set { questionBody = value; }
        }

        public string? QuestionHeader {
            get { return questionHeader ?? "No Header"; }
            set { questionHeader = value; }
        }
        #endregion

        #region Methods
        public override string ToString() {
            return $"Header: {QuestionHeader}\n-------------\nBody: {QuestionBody}\tMark: {Mark}M";
        }

        public abstract void ShowQuestion();

        public virtual bool CheckAnswer() {
            int choice;

            if (AnswerList == null || AnswerList.Length == 0) {
                Console.WriteLine("No Answers Available.");
                return false;
            }
            Console.Write("Enter Your Answer Number: ");

            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > AnswerList.Length) {
                Console.WriteLine("Invalid input, Try Again.");
                Console.Write("Enter Your Answer Number: ");
            }

            return AnswerList[choice - 1].AnswerText == RightAnswer?.AnswerText;
        }
        #endregion

    }

    internal class MCQQuestion : Question {

        #region Constructor
        public MCQQuestion(string header, string body, double mark, Answer[] answers, Answer rightAnswer) {
            QuestionHeader = header;
            QuestionBody = body;
            Mark = mark;
            AnswerList = answers;
            RightAnswer = rightAnswer;
        }
        #endregion

        #region Methods
        public override void ShowQuestion() {
            Console.WriteLine(this.ToString());
            Console.WriteLine("Choices: ");
            if (AnswerList != null) {
                for (int i = 0; i < AnswerList.Length; i++) {
                    Console.WriteLine($"{i + 1}) {AnswerList[i].AnswerText}");
                }
            }
            else {
                Console.WriteLine("No Choices Available.");
            }
        }
        #endregion

    }

    internal class TrueFalseQuestion : Question {

        #region Constructor
        public TrueFalseQuestion(string header, string body, double mark, Answer rightAnswer) {
            QuestionHeader = header;
            QuestionBody = body;
            Mark = mark;
            RightAnswer = rightAnswer;
            // Initialize the answer list with True and False options
            Answer trueAnswer = new Answer(1, "True");
            Answer falseAnswer = new Answer(2, "False");
            AnswerList = new Answer[] { trueAnswer, falseAnswer };
        }
        #endregion

        #region Methods
        public override void ShowQuestion() {
            Console.WriteLine(this.ToString());
            Console.WriteLine("1.True \t 2.False");
        }
        #endregion

    }

    internal abstract class Exam {

        #region Properties
        public int ExamTime { get; set; }
        public DateTime StartExam { get; set; }
        public DateTime EndExam { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int NumberOfQuestions { get; set; }
        public MCQQuestion[]? MCQQuestions { get; set; }
        public double Degree { get; set; }
        public double TotalMark { get; set; }

        #endregion

        #region Constrcutor
        public Exam(int numberOfQuestions, int examTime) {
            NumberOfQuestions = numberOfQuestions;
            ExamTime = examTime;
        }
        #endregion

        #region Methods
        public override string ToString() {
            return $"Exam: {NumberOfQuestions} questions, {ExamTime} minutes";
        }
        public void StartTimer() {
            StartExam = DateTime.Now;
        }
        public void EndTimer() {
            EndExam = DateTime.Now;
        }
        public void CalculateElapsedTime() {
            ElapsedTime = EndExam - StartExam;
            // Make In Nice Format...
            ElapsedTime = new TimeSpan(ElapsedTime.Hours, ElapsedTime.Minutes, ElapsedTime.Seconds);
        }
        public virtual void ShowExam() {
            StartTimer();
            Console.WriteLine($"Exam started at: {StartExam}");
            Console.WriteLine($"Number of questions: {NumberOfQuestions}");
            Console.WriteLine("---------------------------------------\n");
            if (MCQQuestions != null) {
                foreach (Question question in MCQQuestions) {
                    if (IsTimeUp()) {
                        Console.Clear();
                        Console.WriteLine("\nTime is Finish!");
                        break;
                    }
                    question.ShowQuestion();
                    bool isCorrect = question.CheckAnswer();
                    if (isCorrect) {
                        Degree += question.Mark;
                    }
                    TotalMark += question.Mark;
                    Console.Clear();
                }
            }
            else {
                Console.WriteLine("No Questions Available.");
            }
        }
        public virtual void ShowResult() {
            EndTimer();
            CalculateElapsedTime();
            Console.WriteLine($"Exam ended at: {EndExam}");
            Console.WriteLine($"Total time taken: {ElapsedTime.TotalMinutes} minutes");
            Console.WriteLine("-------------------------");
            Console.WriteLine("-- The Correct Answers --");
            Console.WriteLine("-------------------------");
            if (MCQQuestions != null) {
                foreach (Question question in MCQQuestions) {
                    question.ShowQuestion();
                    Console.WriteLine($"The Correct Was: {question.RightAnswer}");
                }
            }
        }
        public void CreateMCQQuestions(int NumberOfQuestions) {
            // Get The Questions
            for (int i = 0; i < NumberOfQuestions; i++) {
                Console.WriteLine($"\n--- MCQ Question {i + 1} ---");

                Console.Write("Enter Question Body: ");
                string? body = Console.ReadLine();

                Console.Write("Enter Question Mark: ");
                double mark = double.TryParse(Console.ReadLine(), out mark) ? mark : 1;

                // Get The Answers
                Answer[] answers = new Answer[4];
                for (int j = 0; j < 4; j++) {
                    string? ansText;
                    bool isDuplicate;

                    // Loop Until The User Enter A Valid Answer
                    do {
                        Console.Write($"Enter Answer {j + 1}: ");
                        ansText = Console.ReadLine();

                        isDuplicate = false;

                        for (int k = 0; k < j; k++) {
                            if (answers[k].AnswerText.Equals(ansText, StringComparison.OrdinalIgnoreCase)) {
                                Console.WriteLine("This Answer Already Exists. Enter a Different One.");
                                isDuplicate = true;
                                break;
                            }
                        }

                        if (String.IsNullOrWhiteSpace(ansText)) { 
                            Console.WriteLine("Answer Cannot Be Empty.");
                            isDuplicate = true;
                        }

                    } while (isDuplicate);

                    answers[j] = new Answer(j + 1, ansText ?? "");
                }

                // Get The Correct Answer
                Console.Write("Enter the correct answer number (1 to 4): ");
                int correctIndex = int.TryParse(Console.ReadLine(), out correctIndex) && correctIndex >= 1 && correctIndex <= 4 ? correctIndex : 1;
                Answer correctAnswer = answers[correctIndex - 1];

                // Complete the question
                MCQQuestion Q = new MCQQuestion("MCQ Question", body ?? "No Body", mark, answers, correctAnswer);

                // Add The Question To The Exam
                MCQQuestions[i] = Q;
            }
        }
        public bool IsTimeUp() {
            return (DateTime.Now - StartExam).TotalMinutes >= ExamTime;
        }
        #endregion
    }

    internal class FinalExam : Exam {

        #region Properties
        public int NumberOfMcq { get; set; }
        public TrueFalseQuestion[]? QuestionsTF { get; set; }
        #endregion

        #region Constructor
        public FinalExam(int numberOfQuestions, int numOfMcq, int examTime) : base(numberOfQuestions, examTime) {
            NumberOfMcq = numOfMcq;
            MCQQuestions = new MCQQuestion[NumberOfMcq];
            QuestionsTF = new TrueFalseQuestion[numberOfQuestions - numOfMcq];
        }
        #endregion

        #region Methods
        public override void ShowExam() {
            base.ShowExam();

            if (QuestionsTF != null) {
                foreach (TrueFalseQuestion question in QuestionsTF) {
                    if (IsTimeUp()) {
                        Console.Clear();
                        Console.WriteLine("\nTime is Finish!");
                        break;
                    }
                    question.ShowQuestion();
                    bool isCorrect = question.CheckAnswer();
                    if (isCorrect) {
                        Degree += question.Mark;
                    }
                    TotalMark += question.Mark;
                    Console.Clear();
                }
            }
            else {
                Console.WriteLine("No True Or False Questions Available.");
            }
        }
        public override void ShowResult() {
            base.ShowResult();
            if (QuestionsTF != null) {
                foreach (Question question in QuestionsTF) {
                    question.ShowQuestion();
                    Console.WriteLine($"The Correct Was: {question.RightAnswer}");
                }
                Console.WriteLine("------------------------");
            }
            Console.WriteLine($"Your Grade Is: {Degree / TotalMark * 100}%");
        }
        public void CreateTFQuestions(int numberOfTFQuestion) {
            // Get The Questions
            for (int i = 0; i < numberOfTFQuestion; i++) {
                Console.WriteLine($"\n--- True/False Question {i + 1} ---");
                Console.Write("Enter Question Body: ");
                string? body = Console.ReadLine();
                Console.Write("Enter Question Mark: ");
                double mark = double.TryParse(Console.ReadLine(), out mark) ? mark : 1;
                // Get The Correct Answer
                Console.Write("Enter the correct answer (True/False): ");
                string? correctAnswerText = Console.ReadLine()?.Trim().ToLower();
                Answer correctAnswer = new Answer(1, correctAnswerText == "true" ? "True" : "False");
                // Complete the question
                TrueFalseQuestion Q = new TrueFalseQuestion("True/False Question", body ?? "No Body", mark, correctAnswer);
                // Add The Question To The Exam
                QuestionsTF[i] = Q;
            }
        }
        public void CreateFinalExam() {
            CreateMCQQuestions(NumberOfMcq);
            CreateTFQuestions(NumberOfQuestions - NumberOfMcq);
            Console.Clear();
            Console.Write("Do You Want To Start Exam? (Y / N): ");
            string? choice = Console.ReadLine()?.Trim().ToLower();
            if (choice == "y" || choice == "yes") {
                Console.Clear();
                ShowExam();
                Console.Clear();
                ShowResult();
            }
            else {
                Console.WriteLine("Exam Cancelled.");
            }
        }
        #endregion

    }

    internal class PracticalExam : Exam {

        #region Constrcutor
        public PracticalExam(int numberOfQuestions, int examTime) : base(numberOfQuestions, examTime) {
            // Ensure MCQQuestions array is initialized to avoid null reference issues
            MCQQuestions = new MCQQuestion[numberOfQuestions];
        }
        #endregion

        #region Method
        public void CreatePracticalExam() {
            CreateMCQQuestions(NumberOfQuestions);
            Console.Clear();
            Console.Write("Do You Want To Start Exam? (Y / N): ");
            string? choice = Console.ReadLine()?.Trim().ToLower();
            if (choice == "y" || choice == "yes") {
                Console.Clear();
                ShowExam();
                Console.Clear();
                ShowResult();
            } else {
                Console.WriteLine("Exam Cancelled.");
            }
        }
        #endregion

    }

    internal class Subject {

        #region Properties
        public string SubjectName { get; set; }
        public int SubjectId { get; set; }
        public string? ExamType { get; set; }
        public Exam? SubjectExam { get; set; }
        #endregion

        #region Constrcutor
        public Subject(string name, int id) {
            SubjectName = name;
            SubjectId = id;
        }
        #endregion

        #region Methods
        public override string ToString() {
            return $"Subject Name: {SubjectName}, Subject ID: {SubjectId}";
        }
        public void CreateExam(string type) {
            if (type.ToLower().Trim() == "practical") {
                Console.Write("The All Questions Will Be MCQ, Enter The Number Of Questions: ");
                int.TryParse(Console.ReadLine(), out int numOfQuestions);
                Console.Write("Enter The Exam Time: ");
                int.TryParse(Console.ReadLine(), out int exTime);
                PracticalExam practicalExam = new PracticalExam(numOfQuestions, exTime);
                SubjectExam = practicalExam;
                practicalExam.CreatePracticalExam();
            }
            else if (type.ToLower().Trim() == "final") {
                Console.Write("Enter The Total Number Of Questions: ");
                int.TryParse(Console.ReadLine(), out int totalQuestions);
                Console.Write("Enter The Number Of MCQ Questions: ");
                int.TryParse(Console.ReadLine(), out int numOfMcq);
                // Check If The Number Of MCQ Questions Exceeds Total Questions
                if (numOfMcq > totalQuestions) {
                    Console.WriteLine("Number of MCQ questions cannot exceed total questions.");
                    return;
                } else {
                    Console.Write("Enter The Exam Time: ");
                    int.TryParse(Console.ReadLine(), out int exTime);
                    FinalExam finalExam = new FinalExam(totalQuestions, numOfMcq, exTime);
                    SubjectExam = finalExam;
                    finalExam.CreateFinalExam();
                }
            }
            else {
                Console.WriteLine("Invalid Exam Type.");
            }
        } 
        #endregion

    }

    internal class Program {
        static void Main(string[] args) {
            // Test This "-"
            Subject S1 = new Subject("Computer Science", 110);
            Console.WriteLine(S1.ToString());
            Console.WriteLine("--------------------------\n");
            Console.Write("Enter Exam Type (Practical/Final): ");
            string? examType = Console.ReadLine();
            S1.CreateExam(examType ?? "Practical");
        }
    }
}
