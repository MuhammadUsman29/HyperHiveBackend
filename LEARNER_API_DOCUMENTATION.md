# Learner Management API Documentation

## 📋 Overview
Complete CRUD API for managing learners with AI profile data that can be easily integrated with frontend forms.

---

## 🔌 API Endpoints

### 1. Get All Learners (with Pagination)
```http
GET /api/learners?page=1&pageSize=10
```

**Query Parameters:**
- `page` (optional, default: 1)
- `pageSize` (optional, default: 10)

**Response:**
```json
{
  "learners": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com",
      "position": "Software Engineer",
      "department": "Engineering",
      "joinedDate": "2024-01-15T00:00:00Z",
      "bio": "Passionate software developer",
      "aiProfile": {
        "skills": ["C#", "React", "Docker"],
        "interests": ["AI/ML", "Cloud Computing"],
        "goals": ["Learn System Design", "Master Kubernetes"],
        "currentLevel": "intermediate",
        "learningStyle": "hands-on",
        "availableHoursPerWeek": 10,
        "preferredLearningTime": "evening",
        "yearsOfExperience": "3",
        "preferredTopics": ["Microservices", "DevOps"],
        "weakAreas": ["System Design", "Algorithms"]
      },
      "createdAt": "2024-11-20T10:00:00Z",
      "updatedAt": "2024-11-20T10:00:00Z"
    }
  ],
  "totalCount": 1
}
```

---

### 2. Get Learner by ID
```http
GET /api/learners/{id}
```

**Response:** Same as single learner object above

---

### 3. Get Learner by Email
```http
GET /api/learners/by-email/{email}
```

**Example:**
```http
GET /api/learners/by-email/john@example.com
```

**Response:** Same as single learner object

---

### 4. Create New Learner
```http
POST /api/learners
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "position": "Software Engineer",
  "department": "Engineering",
  "joinedDate": "2024-01-15T00:00:00Z",
  "bio": "Passionate software developer",
  "aiProfile": {
    "skills": ["C#", "React", "Docker"],
    "interests": ["AI/ML", "Cloud Computing"],
    "goals": ["Learn System Design", "Master Kubernetes"],
    "currentLevel": "intermediate",
    "learningStyle": "hands-on",
    "availableHoursPerWeek": 10,
    "preferredLearningTime": "evening",
    "yearsOfExperience": "3",
    "preferredTopics": ["Microservices", "DevOps"],
    "weakAreas": ["System Design", "Algorithms"]
  }
}
```

**Required Fields:**
- `name` (string)
- `email` (string, unique)
- `position` (string)
- `department` (string)
- `joinedDate` (datetime)

**Optional Fields:**
- `bio` (string)
- `aiProfile` (object)

**Response:** Returns created learner with ID (201 Created)

---

### 5. Update Learner
```http
PUT /api/learners/{id}
Content-Type: application/json
```

**Request Body:** (All fields are optional, only send what needs to be updated)
```json
{
  "name": "John Doe Updated",
  "email": "newemail@example.com",
  "position": "Senior Software Engineer",
  "department": "Engineering",
  "joinedDate": "2024-01-15T00:00:00Z",
  "bio": "Updated bio",
  "aiProfile": {
    "skills": ["C#", "React", "Docker", "Kubernetes"],
    "interests": ["AI/ML", "Cloud Computing", "System Design"],
    "goals": ["Become Solution Architect"],
    "currentLevel": "advanced",
    "learningStyle": "hands-on",
    "availableHoursPerWeek": 15,
    "preferredLearningTime": "morning",
    "yearsOfExperience": "4",
    "preferredTopics": ["Architecture", "Microservices"],
    "weakAreas": ["Frontend Development"]
  }
}
```

**Response:** Returns updated learner (200 OK)

---

### 6. Delete Learner
```http
DELETE /api/learners/{id}
```

**Response:** 204 No Content (success) or 404 Not Found

---

## 📊 AI Profile Structure

The `aiProfile` object contains personalized learning data:

```typescript
interface LearnerAIProfile {
  skills: string[];              // Technical skills they have
  interests: string[];           // Areas they're interested in
  goals: string[];               // Learning goals
  currentLevel: string;          // "beginner" | "intermediate" | "advanced"
  learningStyle: string;         // "visual" | "hands-on" | "reading" | "auditory"
  availableHoursPerWeek: number; // Time available for learning
  preferredLearningTime: string; // "morning" | "afternoon" | "evening"
  yearsOfExperience: string;     // Years in industry
  preferredTopics: string[];     // Topics they want to focus on
  weakAreas: string[];           // Areas they need improvement
}
```

---

## 🎨 Frontend Form Example

### React/Angular Form Fields:

```javascript
const learnerForm = {
  // Basic Info
  name: "",
  email: "",
  position: "",
  department: "",
  joinedDate: new Date(),
  bio: "",
  
  // AI Profile
  aiProfile: {
    skills: [],              // Multi-select dropdown
    interests: [],           // Multi-select dropdown
    goals: [],               // Text area with tags
    currentLevel: "",        // Radio buttons or dropdown
    learningStyle: "",       // Radio buttons or dropdown
    availableHoursPerWeek: 0, // Number input
    preferredLearningTime: "", // Radio buttons or dropdown
    yearsOfExperience: "",   // Number input or dropdown
    preferredTopics: [],     // Multi-select dropdown
    weakAreas: []            // Multi-select dropdown
  }
};
```

---

## 📝 Example Usage

### 1. Create a Learner (Frontend)

```javascript
// Frontend code example
async function createLearner(formData) {
  const response = await fetch('http://localhost:5000/api/learners', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      name: formData.name,
      email: formData.email,
      position: formData.position,
      department: formData.department,
      joinedDate: formData.joinedDate,
      bio: formData.bio,
      aiProfile: {
        skills: formData.skills,
        interests: formData.interests,
        goals: formData.goals,
        currentLevel: formData.currentLevel,
        learningStyle: formData.learningStyle,
        availableHoursPerWeek: formData.availableHours,
        preferredLearningTime: formData.preferredTime,
        yearsOfExperience: formData.experience,
        preferredTopics: formData.topics,
        weakAreas: formData.weakAreas
      }
    })
  });
  
  if (response.ok) {
    const learner = await response.json();
    console.log('Created learner:', learner);
    return learner;
  } else {
    const error = await response.json();
    console.error('Error:', error);
    throw new Error(error.error);
  }
}
```

### 2. Update Learner Profile

```javascript
async function updateLearnerProfile(learnerId, updates) {
  const response = await fetch(`http://localhost:5000/api/learners/${learnerId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(updates)
  });
  
  return await response.json();
}
```

### 3. Get Learner for Quiz

```javascript
async function getLearnerForQuiz(learnerId) {
  const response = await fetch(`http://localhost:5000/api/learners/${learnerId}`);
  const learner = await response.json();
  
  // Use learner.aiProfile to generate quiz
  return learner;
}
```

---

## ✅ Validation

### Email Validation
- Email must be unique
- Cannot create/update with duplicate email

### Required Fields (Create)
- name
- email
- position
- department
- joinedDate

### Optional Fields
All `aiProfile` fields are optional but recommended for better AI-generated content

---

## 🔄 Complete Flow

1. **User fills form** → Frontend collects data
2. **POST /api/learners** → Create learner
3. **Backend stores** → Saves to database with AI profile as JSON
4. **Generate quiz** → Use learner ID with `/api/quiz/generate`
5. **AI reads profile** → OpenAI generates personalized quiz based on AI profile
6. **Return quiz** → Frontend displays quiz

---

## 🎯 Next Steps

After creating learners, you can:
1. Generate personalized quizzes using their AI profile
2. Track their progress over time
3. Recommend learning paths
4. Create dashboards showing skill development

---

Ready to test! 🚀

