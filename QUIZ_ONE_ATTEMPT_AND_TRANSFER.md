# Quiz Flow: One Attempt & Frontend Data Transfer

## ✅ **1. ONE QUIZ ATTEMPT PER LEARNER**

### **How It Works:**

When a learner tries to submit a quiz, the backend now checks if they've already attempted it:

```csharp
// In SubmitQuizAsync method:
var existingAttempt = await _context.QuizAttempts
    .FirstOrDefaultAsync(a => a.QuizId == request.QuizId && a.LearnerId == request.LearnerId);

if (existingAttempt != null)
{
    throw new Exception("You have already attempted this quiz. Each quiz can only be taken once.");
}
```

### **API Response When Already Attempted:**

```http
POST /api/quiz/submit
```

**Response (500 Error):**
```json
{
  "error": "Failed to submit quiz",
  "details": "You have already attempted this quiz. Each quiz can only be taken once."
}
```

### **Check Before Showing Quiz:**

Frontend can check if learner already attempted a quiz BEFORE showing it:

```http
GET /api/quiz/{quizId}/attempted/{learnerId}
```

**Example:**
```http
GET /api/quiz/123/attempted/1
```

**Response:**
```json
{
  "hasAttempted": false
}
```

or

```json
{
  "hasAttempted": true
}
```

### **Frontend Flow with One Attempt:**

```javascript
// Step 1: Generate quiz
const quiz = await generateQuiz(learnerId);

// Step 2: Check if already attempted (optional, for UI)
const check = await fetch(`/api/quiz/${quiz.quizId}/attempted/${learnerId}`);
const { hasAttempted } = await check.json();

if (hasAttempted) {
  alert("You've already taken this quiz!");
  // Show results instead
  return;
}

// Step 3: Show quiz
displayQuiz(quiz);

// Step 4: User answers and submits
try {
  const results = await submitQuiz(quiz.quizId, learnerId, answers);
  showResults(results);
} catch (error) {
  if (error.details.includes("already attempted")) {
    alert("You've already taken this quiz!");
  }
}
```

---

## 📡 **2. HOW QUIZ IS RETURNED TO FRONTEND**

### **Complete Data Transfer Explanation:**

The quiz travels from backend to frontend as **JSON data over HTTP**.

### **Step-by-Step Data Transfer:**

#### **1️⃣ Generate Quiz API Call:**

**Frontend Makes Request:**
```javascript
const response = await fetch('http://localhost:5000/api/quiz/generate', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    learnerId: 1,
    quizType: 'SkillAssessment',
    numberOfQuestions: 5
  })
});
```

#### **2️⃣ Backend Processes:**

```
Backend receives request
    ↓
Fetches Learner from database
    ↓
Gets learner's AI profile data
    ↓
Sends profile to OpenAI
    ↓
OpenAI generates personalized quiz
    ↓
Backend stores quiz in database (with correct answers)
    ↓
Backend prepares response (without correct answers)
    ↓
Converts to JSON
    ↓
Sends HTTP response
```

#### **3️⃣ Frontend Receives JSON Response:**

```javascript
const quiz = await response.json();

// quiz now contains:
console.log(quiz);
```

**Output:**
```json
{
  "quizId": 123,
  "title": "C# Skills Assessment",
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
      "question": "Which keyword is used to define async methods?",
      "options": [
        "async",
        "await",
        "promise",
        "defer"
      ],
      "type": "multiple-choice"
    }
  ]
}
```

#### **4️⃣ Frontend Uses This Data:**

```javascript
// Access quiz data
const quizTitle = quiz.title;  // "C# Skills Assessment"
const quizId = quiz.quizId;    // 123

// Loop through questions
quiz.questions.forEach((question, index) => {
  console.log(`Question ${index + 1}: ${question.question}`);
  console.log(`Options:`, question.options);
});

// Display in UI
function displayQuiz(quiz) {
  document.getElementById('quiz-title').textContent = quiz.title;
  
  const questionsContainer = document.getElementById('questions');
  
  quiz.questions.forEach(q => {
    const questionDiv = document.createElement('div');
    questionDiv.innerHTML = `
      <h3>${q.question}</h3>
      ${q.options.map(option => `
        <label>
          <input type="radio" name="question-${q.questionId}" value="${option}">
          ${option}
        </label>
      `).join('')}
    `;
    questionsContainer.appendChild(questionDiv);
  });
}
```

---

## 🔄 **Complete Visual Flow:**

```
┌─────────────────────────────────────────────────────────┐
│              FRONTEND (Angular/React)                    │
│                                                          │
│  User clicks "Start Quiz"                               │
│         ↓                                                │
│  JavaScript sends HTTP POST request                     │
│         ↓                                                │
│  fetch('/api/quiz/generate', {...})                     │
└─────────────────────────────────────────────────────────┘
                         │
                    HTTP Request
                    (over network)
                         │
                         ↓
┌─────────────────────────────────────────────────────────┐
│              BACKEND (ASP.NET Core)                      │
│                                                          │
│  QuizController receives request                        │
│         ↓                                                │
│  QuizService.GenerateQuizAsync()                        │
│         ↓                                                │
│  Fetch Learner from MySQL                               │
│         ↓                                                │
│  Send to OpenAI API                                     │
│         ↓                                                │
│  OpenAI returns personalized quiz                       │
│         ↓                                                │
│  Store quiz in MySQL database                           │
│         ↓                                                │
│  Create response object (GenerateQuizResponse)          │
│         ↓                                                │
│  Serialize to JSON                                      │
│         ↓                                                │
│  Send HTTP Response                                     │
└─────────────────────────────────────────────────────────┘
                         │
                    HTTP Response
                    Content-Type: application/json
                    Body: {...quiz data...}
                         │
                         ↓
┌─────────────────────────────────────────────────────────┐
│              FRONTEND (Angular/React)                    │
│                                                          │
│  await response.json()                                  │
│         ↓                                                │
│  quiz = { quizId: 123, title: "...", questions: [...] }│
│         ↓                                                │
│  Store quiz in state/variable                           │
│         ↓                                                │
│  Render quiz UI                                         │
│         ↓                                                │
│  User sees questions and options                        │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 **What Data is Sent:**

### **From Frontend to Backend (Generate):**
```json
{
  "learnerId": 1,
  "quizType": "SkillAssessment",
  "numberOfQuestions": 5
}
```

### **From Backend to Frontend (Quiz):**
```json
{
  "quizId": 123,
  "title": "C# Skills Assessment",
  "questions": [
    {
      "questionId": 1,
      "question": "What is...",
      "options": ["A", "B", "C", "D"],
      "type": "multiple-choice"
    }
  ]
}
```
**⚠️ NOTE:** Correct answers are NOT included here!

### **From Frontend to Backend (Submit):**
```json
{
  "quizId": 123,
  "learnerId": 1,
  "answers": [
    {
      "questionId": 1,
      "selectedAnswer": "A design pattern..."
    },
    {
      "questionId": 2,
      "selectedAnswer": "async"
    }
  ]
}
```

### **From Backend to Frontend (Results):**
```json
{
  "attemptId": 456,
  "score": 4,
  "totalQuestions": 5,
  "percentage": 80.00,
  "feedback": "Great job!...",
  "results": [
    {
      "questionId": 1,
      "question": "What is...",
      "yourAnswer": "A design pattern...",
      "correctAnswer": "A design pattern...",
      "isCorrect": true,
      "explanation": "Because..."
    }
  ]
}
```
**✅ NOTE:** NOW correct answers are included!

---

## 🛡️ **Security Notes:**

1. **Correct answers are stored ONLY on backend**
2. **Frontend never sees correct answers until submission**
3. **One attempt per quiz prevents cheating**
4. **All validation happens on backend**

---

## 🎯 **Frontend Integration Example:**

### **React/Angular Component:**

```typescript
import { Component } from '@angular/core';
import { QuizService } from './quiz.service';

@Component({
  selector: 'app-quiz',
  template: `
    <div *ngIf="quiz">
      <h2>{{ quiz.title }}</h2>
      
      <div *ngFor="let question of quiz.questions">
        <h3>{{ question.question }}</h3>
        <div *ngFor="let option of question.options">
          <label>
            <input 
              type="radio" 
              [name]="'q-' + question.questionId"
              [value]="option"
              (change)="selectAnswer(question.questionId, option)"
            >
            {{ option }}
          </label>
        </div>
      </div>
      
      <button (click)="submitQuiz()">Submit Quiz</button>
    </div>
    
    <div *ngIf="results">
      <h2>Results: {{ results.score }} / {{ results.totalQuestions }}</h2>
      <p>{{ results.feedback }}</p>
      
      <div *ngFor="let result of results.results">
        <div [class.correct]="result.isCorrect" [class.wrong]="!result.isCorrect">
          <p>{{ result.question }}</p>
          <p>Your answer: {{ result.yourAnswer }}</p>
          <p *ngIf="!result.isCorrect">Correct: {{ result.correctAnswer }}</p>
          <p>{{ result.explanation }}</p>
        </div>
      </div>
    </div>
  `
})
export class QuizComponent {
  quiz: any;
  answers: Map<number, string> = new Map();
  results: any;
  learnerId = 1;  // From user session

  constructor(private quizService: QuizService) {}

  async ngOnInit() {
    // Generate quiz
    this.quiz = await this.quizService.generateQuiz(this.learnerId);
  }

  selectAnswer(questionId: number, answer: string) {
    this.answers.set(questionId, answer);
  }

  async submitQuiz() {
    const answersArray = Array.from(this.answers.entries()).map(([questionId, selectedAnswer]) => ({
      questionId,
      selectedAnswer
    }));

    this.results = await this.quizService.submitQuiz(
      this.quiz.quizId,
      this.learnerId,
      answersArray
    );
  }
}
```

---

## ✅ **Summary:**

1. **One Attempt:** Backend now enforces one quiz attempt per learner
2. **Data Transfer:** Quiz is sent as JSON over HTTP
3. **Security:** Correct answers only revealed after submission
4. **Frontend:** Receives JSON, parses it, displays in UI

**The quiz literally travels through the internet as text (JSON) and arrives at the frontend where JavaScript converts it into a usable object!** 🚀

