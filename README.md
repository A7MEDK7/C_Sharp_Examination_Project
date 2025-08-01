# 📝 Exam Management System using C# (Basics And OOP Principles)

Welcome to my first backend project built using C# and Object-Oriented Programming (OOP)!  
This console-based exam system simulates real-life exams including MCQ and True/False questions. It supports both **Practical** and **Final** exams, with **timing**, **auto-grading**, and **result display**.

---

## 🚀 Features

- 🧠 **Object-Oriented Design (OOP)**: Abstraction, Inheritance, Polymorphism, Encapsulations
- ❓ **MCQ & True/False Question Support**
- ⏱️ **Exam Timer** with real-time countdown and auto-termination
- ✅ **Auto-Grading** and percentage calculation
- 🧪 **Final & Practical Exam Modes**
- 🔁 **Answer Validation**: Prevents duplicate answer inputs
- 📄 **Dynamic Question Creation** through console

---

## 🛠️ Technologies Used

- Language: **C#**
- Type: **Console Application**

---

## Class Digram Of The Project
![Class Digram](Images/Class_Digram.png)

---

## 📂 Project Structure

```plaintext
├── Answer.cs               # Represents an answer option
├── Question.cs             # Abstract base class for all questions
├── MCQQuestion.cs          # Multiple choice question implementation
├── TrueFalseQuestion.cs    # True/False question implementation
├── Exam.cs                 # Base class for handling exam logic
├── FinalExam.cs            # Supports mixed MCQ + TF questions
├── PracticalExam.cs        # MCQ-only practical exams
├── Subject.cs              # Represents a subject and manages its exam
└── Program.cs              # Entry point and user interaction
