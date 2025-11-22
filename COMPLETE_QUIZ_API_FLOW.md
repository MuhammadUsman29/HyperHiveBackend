# Complete Quiz API Documentation for Frontend

## 🔌 All Quiz API Endpoints

---

## **1. Generate Quiz (Send to Frontend)**
Creates a personalized AI-generated quiz for a learner.

```http
POST /api/quiz/generate
Content-Type: application/json
```

**Request Body:**
```json
{
  "learnerId": 1,
  "quizType": "SkillAssessment",
  "difficulty": "intermediate",
  "numberOfQuestions": 5
}
```

**Response (200 OK):**
```json
{
  "quizId": 123,
  "title": "C# and React Skills Assessment",
  "questions": [
    {
      "questionId": 1,
      "question": "What is dependency injection in C#?",
      "options": [
        "A design pattern for managing dependencies",
        "A type of SQL injection",
        "A testing framework",
        "A compiler optimization"
      ],
      "type": "multiple-choice"
    },
    {
      "questionId": 2,
      "question": "In React, what hook is used for side effects?",
      "options": [
        "useEffect",
        "useState",
        "useContext",
        "useMemo"
      ],
      "type": "multiple-choice"
    }
  ]
}
```

**Note:** Correct answers are NOT included in this response (security).

---

## **2. Submit Quiz & Get Results**
Submits learner's answers, validates them, saves results, and returns detailed feedback.

```http
POST /api/quiz/submit
Content-Type: application/json
```

**Request Body:**
```json
{
  "quizId": 123,
  "learnerId": 1,
  "answers": [
    {
      "questionId": 1,
      "selectedAnswer": "A design pattern for managing dependencies"
    },
    {
      "questionId": 2,
      "selectedAnswer": "useEffect"
    },
    {
      "questionId": 3,
      "selectedAnswer": "Wrong answer"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "attemptId": 456,
  "score": 4,
  "totalQuestions": 5,
  "percentage": 80.00,
  "feedback": "Great job! You're doing well, but there's room for improvement.",
  "results": [
    {
      "questionId": 1,
      "question": "What is dependency injection in C#?",
      "yourAnswer": "A design pattern for managing dependencies",
      "correctAnswer": "A design pattern for managing dependencies",
      "isCorrect": true,
      "explanation": "Dependency injection is a design pattern that allows classes to receive their dependencies from external sources..."
    },
    {
      "questionId": 2,
      "question": "In React, what hook is used for side effects?",
      "yourAnswer": "useEffect",
      "correctAnswer": "useEffect",
      "isCorrect": true,
      "explanation": "useEffect is the React hook for handling side effects like data fetching, subscriptions, or DOM manipulation."
    },
    {
      "questionId": 3,
      "question": "...",
      "yourAnswer": "Wrong answer",
      "correctAnswer": "Correct answer",
      "isCorrect": false,
      "explanation": "The correct answer is..."
    }
  ]
}
```

---

## **3. Get Quiz Attempt Results (View Past Results)**
Retrieve detailed results for a specific quiz attempt (in case user wants to review later).

```http
GET /api/quiz/attempt/{attemptId}
```

**Example:**
```http
GET /api/quiz/attempt/456
```

**Response (200 OK):**
```json
{
  "attemptId": 456,
  "score": 4,
  "totalQuestions": 5,
  "percentage": 80.00,
  "feedback": "Great job!...",
  "results": [
    // Same format as submit response
  ]
}
```

**Use Case:** User clicks "Review Results" from their history

---

## **4. Get All Attempts for a Learner (History)**
Get all quiz attempts for a specific learner.

```http
GET /api/quiz/learner/{learnerId}/attempts
```

**Example:**
```http
GET /api/quiz/learner/1/attempts
```

**Response (200 OK):**
```json
[
  {
    "attemptId": 456,
    "quizId": 123,
    "quizTitle": "C# Skills Assessment",
    "quizType": "SkillAssessment",
    "score": 4,
    "totalQuestions": 5,
    "percentage": 80.00,
    "completedAt": "2024-11-22T14:30:00Z",
    "timeTakenSeconds": 300
  },
  {
    "attemptId": 455,
    "quizId": 122,
    "quizTitle": "React Fundamentals",
    "quizType": "KnowledgeCheck",
    "score": 5,
    "totalQuestions": 5,
    "percentage": 100.00,
    "completedAt": "2024-11-21T10:15:00Z",
    "timeTakenSeconds": 240
  }
]
```

**Use Case:** Display quiz history on learner dashboard

---

## **5. Get Learner Statistics (Progress Dashboard)**
Get comprehensive statistics for a learner's quiz performance.

```http
GET /api/quiz/learner/{learnerId}/statistics
```

**Example:**
```http
GET /api/quiz/learner/1/statistics
```

**Response (200 OK):**
```json
{
  "learnerId": 1,
  "totalQuizzesTaken": 15,
  "averageScore": 78.5,
  "bestScore": 10,
  "totalQuestionsAnswered": 75,
  "totalCorrectAnswers": 59,
  "recentAttempts": [
    {
      "attemptId": 456,
      "quizId": 123,
      "quizTitle": "C# Skills Assessment",
      "quizType": "SkillAssessment",
      "score": 4,
      "totalQuestions": 5,
      "percentage": 80.00,
      "completedAt": "2024-11-22T14:30:00Z",
      "timeTakenSeconds": 300
    }
    // ... up to 10 most recent attempts
  ]
}
```

**Use Case:** Display progress charts, statistics on dashboard

---

## **6. Get Quiz Details (Metadata)**
Get quiz metadata without questions/answers.

```http
GET /api/quiz/{quizId}
```

**Example:**
```http
GET /api/quiz/123
```

**Response (200 OK):**
```json
{
  "quizId": 123,
  "title": "C# Skills Assessment",
  "quizType": "SkillAssessment",
  "difficulty": "intermediate",
  "generatedAt": "2024-11-22T10:00:00Z",
  "totalQuestions": 5,
  "timesAttempted": 3
}
```

**Use Case:** Display quiz info before retaking

---

## 🎯 **Complete Frontend Flow**

### **Step 1: User Creates Profile**
```javascript
// POST /api/learners
const learner = await createLearner(formData);
// Save learnerId: 1
```

### **Step 2: Generate Quiz**
```javascript
// POST /api/quiz/generate
const quiz = await fetch('/api/quiz/generate', {
  method: 'POST',
  body: JSON.stringify({
    learnerId: 1,
    quizType: 'SkillAssessment',
    numberOfQuestions: 5
  })
});
// Receive quiz with questions (no correct answers)
// Save quizId: 123
```

### **Step 3: Display Quiz**
```javascript
// Show questions to user
quiz.questions.map(q => (
  <QuizQuestion 
    question={q.question}
    options={q.options}
    onAnswer={(answer) => saveAnswer(q.questionId, answer)}
  />
));
```

### **Step 4: Submit Answers**
```javascript
// POST /api/quiz/submit
const results = await fetch('/api/quiz/submit', {
  method: 'POST',
  body: JSON.stringify({
    quizId: 123,
    learnerId: 1,
    answers: selectedAnswers
  })
});
// Receive results with score, feedback, and detailed breakdown
// Save attemptId: 456
```

### **Step 5: Display Results**
```javascript
// Show score and detailed results
<ResultsPage>
  <Score>{results.score} / {results.totalQuestions}</Score>
  <Percentage>{results.percentage}%</Percentage>
  <Feedback>{results.feedback}</Feedback>
  
  {results.results.map(r => (
    <QuestionResult
      question={r.question}
      yourAnswer={r.yourAnswer}
      correctAnswer={r.correctAnswer}
      isCorrect={r.isCorrect}
      explanation={r.explanation}
    />
  ))}
</ResultsPage>
```

### **Step 6: View History (Optional)**
```javascript
// GET /api/quiz/learner/1/attempts
const history = await fetch('/api/quiz/learner/1/attempts');

// Display history
<QuizHistory>
  {history.map(attempt => (
    <AttemptCard
      title={attempt.quizTitle}
      score={attempt.score}
      percentage={attempt.percentage}
      date={attempt.completedAt}
      onClick={() => viewResults(attempt.attemptId)}
    />
  ))}
</QuizHistory>
```

### **Step 7: View Past Results (Optional)**
```javascript
// GET /api/quiz/attempt/456
const pastResults = await fetch('/api/quiz/attempt/456');
// Display same results page as Step 5
```

### **Step 8: View Statistics (Optional)**
```javascript
// GET /api/quiz/learner/1/statistics
const stats = await fetch('/api/quiz/learner/1/statistics');

// Display dashboard
<Dashboard>
  <Stat>Total Quizzes: {stats.totalQuizzesTaken}</Stat>
  <Stat>Average Score: {stats.averageScore}%</Stat>
  <Stat>Best Score: {stats.bestScore}</Stat>
  <Chart data={stats.recentAttempts} />
</Dashboard>
```

---

## 📊 **Data Flow Summary**

```
FRONTEND                    BACKEND                     DATABASE
--------                    -------                     --------

1. Create Learner
   POST /api/learners  →    Creates Learner      →     Learners Table
   ← Returns learnerId

2. Generate Quiz
   POST /api/quiz/generate → Fetch Learner       ←     Learners Table
                          → Send to OpenAI
                          ← AI generates quiz
                          → Store quiz          →     Quizzes Table
   ← Returns quiz (no answers)

3. Submit Quiz
   POST /api/quiz/submit  → Fetch quiz          ←     Quizzes Table
                          → Validate answers
                          → Calculate score
                          → Store attempt       →     QuizAttempts Table
   ← Returns results with correct answers

4. View History
   GET /api/quiz/learner/1/attempts  
                          → Fetch attempts      ←     QuizAttempts Table
   ← Returns list of attempts

5. View Results
   GET /api/quiz/attempt/456
                          → Fetch attempt       ←     QuizAttempts Table
                          → Fetch quiz          ←     Quizzes Table
   ← Returns detailed results

6. View Statistics
   GET /api/quiz/learner/1/statistics
                          → Calculate stats     ←     QuizAttempts Table
   ← Returns aggregated statistics
```

---

## ✅ **Complete API List**

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/learners` | Create learner profile |
| GET | `/api/learners/{id}` | Get learner by ID |
| POST | `/api/quiz/generate` | Generate AI quiz |
| POST | `/api/quiz/submit` | Submit answers & get results |
| GET | `/api/quiz/attempt/{attemptId}` | View past results |
| GET | `/api/quiz/learner/{learnerId}/attempts` | Get quiz history |
| GET | `/api/quiz/learner/{learnerId}/statistics` | Get statistics |
| GET | `/api/quiz/{quizId}` | Get quiz metadata |

---

## 🚀 **Ready for Frontend Integration!**

All backend APIs are now complete and ready to be consumed by your Angular frontend. The flow is:
1. ✅ Create learner
2. ✅ Generate quiz
3. ✅ Display quiz
4. ✅ Submit & get results
5. ✅ View history & statistics

Let me know if you need any clarifications! 🎯

